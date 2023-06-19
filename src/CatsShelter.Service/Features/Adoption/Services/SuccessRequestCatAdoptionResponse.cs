namespace CatsShelter.Service.Features.Adoption.Services;

public class SuccessRequestCatAdoptionResponse : CatAdoptionResponse
{
    private const string RequestAdoptionSuccessMessage = "Adoption successful.";

    public SuccessRequestCatAdoptionResponse() : base(true, RequestAdoptionSuccessMessage)
    {
    }
}