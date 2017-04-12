using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Douyu.Messages
{
    public class BarrageConverter : DouyuMessageConverter<Barrage>
    {
        public override string DumpsString(Barrage douyuMessage)
        {
            throw new NotImplementedException();
        }
    }
}
