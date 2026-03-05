namespace Nop.Plugin.Misc.WebApi.Framework.Dto
{
    public abstract class ModelWithIdDto : ModelDto
    {
        /// <summary>
        /// Gets or sets the dto object identifier
        /// </summary>
        public int Id { get; set; }
    }
}
