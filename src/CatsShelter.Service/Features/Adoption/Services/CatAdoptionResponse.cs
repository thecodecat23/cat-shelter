namespace CatsShelter.Service.Features.Adoption.Services;

public class CatAdoptionResponse
{
    public CatAdoptionResponse(bool isSuccess, string message)
    {
        IsSuccess = isSuccess;
        Message = message;
    }

    public bool IsSuccess { get; set; }
    public string Message { get; set; }
}
