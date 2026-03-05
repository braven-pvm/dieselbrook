namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Vendors
{
    public partial class InfoRequest : BaseModelDtoRequest<VendorInfoModelDto>
    {
        public byte[] PictureBinary { get; set; }
    }
}
