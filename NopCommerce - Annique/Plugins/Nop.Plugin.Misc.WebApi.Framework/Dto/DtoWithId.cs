namespace Nop.Plugin.Misc.WebApi.Framework.Dto
{
    public abstract class DtoWithId : BaseDto
    {
        /// <summary>
        /// Gets or sets the dto object identifier
        /// </summary>
        public int Id { get; set; }
    }
}
