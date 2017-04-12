using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DouyuDanmu.Messages
{
    public class BarrageConverter : DouyuMessageConverter<Barrage>
    {
        public static BarrageConverter @Default = new BarrageConverter();
        public override string DumpsString(Barrage douyuMessage)
        {
            throw new NotImplementedException();
        }
    }
}
