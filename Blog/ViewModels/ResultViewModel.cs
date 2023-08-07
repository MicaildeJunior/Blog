namespace Blog.ViewModels;

public class ResultViewModel<T>
{
    public ResultViewModel(T data, List<string> errors)
    {
        Data = data;
        Errors = errors;
    }

    // Só recebe o sucesso, os dados se der certo
    public ResultViewModel(T data)
    {
        Data = data;
    }

    // Quando tem só uma List de Erro
    public ResultViewModel(List<string> errors)
    {
        Errors = errors;
    }

    // Quando tem só um Erro
    public ResultViewModel(string errors)
    {
        Errors.Add(errors);
    }

    // private para não alterar os itens
    public T Data { get; private set; }
    public List<string> Errors { get; private set; } = new();
}
