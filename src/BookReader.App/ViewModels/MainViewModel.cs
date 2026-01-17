using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Media.Imaging;
using BookReader.Domain;
using BookReader.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace BookReader.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly AppDbContext _db = AppDbContext.Create();

    [ObservableProperty] private string _status = "Připraveno";
    [ObservableProperty] private string _listeningHint = "Povězte: „vyfoť“, „ulož“, „autor: …“";

    [ObservableProperty] private BookEditModel _edit = BookEditModel.Empty();
    [ObservableProperty] private BitmapSource? _cameraImage;

    public MainViewModel()
    {
        // Auto-vytvoř DB + seed číselníku při prvním spuštění
        _db.Database.Migrate();
        if (!_db.MainGroups.Any())
        {
            _db.MainGroups.AddRange(new[]
            {
                new MainGroup { Id = 1, Name = "Beletrie" },
                new MainGroup { Id = 2, Name = "Naučná" },
                new MainGroup { Id = 3, Name = "Dětská" },
                new MainGroup { Id = 4, Name = "Technická" },
                new MainGroup { Id = 5, Name = "Historie" }
            });
            _db.SaveChanges();
        }
    }

    [RelayCommand]
    private void New()
    {
        Edit = BookEditModel.Empty();
        Status = "Nový záznam";
    }

    [RelayCommand]
    private async Task Save()
    {
        var entity = Edit.ToEntity();
        var existing = await _db.Books.Include(b => b.BookTags).FirstOrDefaultAsync(b => b.Id == entity.Id);
        if (existing is null)
        {
            entity.CreatedUtc = DateTime.UtcNow;
            entity.UpdatedUtc = DateTime.UtcNow;
            await _db.Books.AddAsync(entity);
        }
        else
        {
            existing.CopyFrom(entity);
            existing.UpdatedUtc = DateTime.UtcNow;
        }

        // Tagy
        var tags = Edit.ParseTags();
        var tagEntities = await _db.Tags.Where(t => tags.Contains(t.Name)).ToListAsync();
        var missing = tags.Except(tagEntities.Select(t => t.Name)).ToArray();
        foreach (var m in missing) tagEntities.Add(new Tag { Name = m });
        entity.BookTags = tagEntities.Select(t => new BookTag { BookId = entity.Id, Tag = t }).ToList();

        await _db.SaveChangesAsync();
        Status = "Uloženo";
    }

    [RelayCommand]
    private async Task CaptureCover()
    {
        Status = "Pořizuji snímek…";
        var frame = await Services.CameraService.CaptureAsync();
        if (frame is not null)
        {
            var (coverPath, thumbnail) = await Services.ImageService.SaveCoverAsync(Edit.Id, frame);
            Edit.CoverPath = coverPath;
            CameraImage = thumbnail;
            Status = "Snímek hotov";
        }
        else Status = "Kamera nedostupná";
    }
}

public class BookEditModel : ObservableObject
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Title { get; set; }
    public string? Authors { get; set; }
    public int? Year { get; set; }
    public string? Publisher { get; set; }
    public int? WidthMm { get; set; }
    public int? HeightMm { get; set; }
    public int? DepthMm { get; set; }
    public string? Annotation { get; set; }
    public string? Tags { get; set; }
    public string? MainGroup { get; set; }
    public string? Isbn { get; set; }
    public string? CoverPath { get; set; }

    public static BookEditModel Empty() => new();

    public IEnumerable<string> ParseTags() =>
        (Tags ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase);

    public Book ToEntity() => new()
    {
        Id = Id,
        Title = Title?.Trim() ?? "",
        Authors = Authors?.Trim() ?? "",
        Year = Year,
        Publisher = Publisher?.Trim(),
        WidthMm = WidthMm,
        HeightMm = HeightMm,
        DepthMm = DepthMm,
        Annotation = Annotation,
        MainGroupId = null, // mapování podle číselníku doplníme později
        Isbn = Isbn,
        CoverPath = CoverPath
    };

    public void FromEntity(Book b)
    {
        Id = b.Id;
        Title = b.Title;
        Authors = b.Authors;
        Year = b.Year;
        Publisher = b.Publisher;
        WidthMm = b.WidthMm;
        HeightMm = b.HeightMm;
        DepthMm = b.DepthMm;
        Annotation = b.Annotation;
        Isbn = b.Isbn;
        CoverPath = b.CoverPath;
    }
}
