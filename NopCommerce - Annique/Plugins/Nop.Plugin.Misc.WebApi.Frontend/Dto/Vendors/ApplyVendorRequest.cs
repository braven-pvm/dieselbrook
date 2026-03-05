namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Vendors
{
    public partial class ApplyVendorRequest : BaseModelDtoRequest<ApplyVendorModelDto>
    {
        public byte[] PictureBinary { get; set; }
    }
}
