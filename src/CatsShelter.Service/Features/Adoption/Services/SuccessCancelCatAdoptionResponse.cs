namespace CatsShelter.Service.Features.Adoption.Services;

public class SuccessCancelCatAdoptionResponse : CatAdoptionResponse
{
    private const string CancelAdoptionSuccessMessage = "Adoption canceled.";

    public SuccessCancelCatAdoptionResponse() : base(true, CancelAdoptionSuccessMessage)
    {
    }
}