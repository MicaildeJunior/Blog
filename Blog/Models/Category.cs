namespace Blog.Models;

public class Category
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Slug { get; set; }

    // IList é sempre inicializada e não vem nula , se fosse IEnumerable viria null sem cadastrar nada
    public IList<Post> Posts { get; set; }
}