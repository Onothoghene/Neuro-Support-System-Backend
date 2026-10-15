namespace Application.Interfaces
{
    public interface IOAuthCompletionTokenService
    {
        string Generate(string email, string provider);
        (string email, string provider)? Validate(string token);
        void Consume(string token);
    }
}
