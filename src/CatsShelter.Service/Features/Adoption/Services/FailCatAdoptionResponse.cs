namespace CatsShelter.Service.Features.Adoption.Services;

public class FailCatAdoptionResponse : CatAdoptionResponse
{
    public FailCatAdoptionResponse(Exception exception) : base(false, exception.Message)
    {
    }
}
