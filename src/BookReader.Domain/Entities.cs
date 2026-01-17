namespace BookReader.Domain;

public class Book
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string Authors { get; set; } = ""; // CSV „Jméno1, Jméno2“
    public string? Publisher { get; set; }
    public int? Year { get; set; }

    public int? WidthMm { get; set; }
    public int? HeightMm { get; set; }
    public int? DepthMm { get; set; }

    public string? Annotation { get; set; }
    public string? Isbn { get; set; }
    public string? CoverPath { get; set; }

    public int? MainGroupId { get; set; }
    public MainGroup? MainGroup { get; set; }

    public List<BookTag> BookTags { get; set; } = new();
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }

    public void CopyFrom(Book src)
    {
        Title = src.Title;
        Authors = src.Authors;
        Publisher = src.Publisher;
        Year = src.Year;
        WidthMm = src.WidthMm;
        HeightMm = src.HeightMm;
        DepthMm = src.DepthMm;
        Annotation = src.Annotation;
        Isbn = src.Isbn;
        CoverPath = src.CoverPath;
        MainGroupId = src.MainGroupId;
    }
}

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public List<BookTag> BookTags { get; set; } = new();
}

public class BookTag
{
    public Guid BookId { get; set; }
    public Book Book { get; set; } = default!;
    public int TagId { get; set; }
    public Tag Tag { get; set; } = default!;
}

public class MainGroup
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public List<Book> Books { get; set; } = new();
}
