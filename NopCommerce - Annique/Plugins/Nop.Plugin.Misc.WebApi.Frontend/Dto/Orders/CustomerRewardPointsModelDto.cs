using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Common;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Orders
{
    public partial class CustomerRewardPointsModelDto : ModelDto
    {
        public IList<RewardPointsHistoryModelDto> RewardPoints { get; set; }

        public PagerModelDto PagerModel { get; set; }

        public int RewardPointsBalance { get; set; }

        public string RewardPointsAmount { get; set; }

        public int MinimumRewardPointsBalance { get; set; }

        public string MinimumRewardPointsAmount { get; set; }
    }
}
