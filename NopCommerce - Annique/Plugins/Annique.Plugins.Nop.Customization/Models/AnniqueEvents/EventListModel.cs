using Nop.Web.Framework.Models;
using Nop.Web.Models.Media;
using System.Collections.Generic;

namespace Annique.Plugins.Nop.Customization.Models.AnniqueEvents
{
    public record EventListModel : BaseNopModel
    {
        #region Ctor

        public EventListModel()
        {
            Events = new List<EventDetailsModel>();
        }

        #endregion

        #region Property

        public IList<EventDetailsModel> Events { get; set; }

        #endregion

        #region Nested Class

        public record EventDetailsModel : BaseNopModel
        {
            public EventDetailsModel()
            {
                PictureModel = new PictureModel();
            }
            public int Id { get; set; }

            public string Name { get; set; }    

            public string Description { get; set; }

            public string Date { get; set; }

            public string Time { get; set; }

            public string Price { get; set; }

            public bool BookingOpen { get; set; }

            public string LocationAddress1 { get; set; }

            public string LocationAddress2 { get; set; }

            public PictureModel PictureModel { get; set; }
        }

        #endregion
    }
}
