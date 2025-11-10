public class ProdutoNaoEncontradoError : Exception
{
    public CustomException(string message) : base(message)
    {
        throw new System.Exception("Produto com id não encontrado");
    }
}
