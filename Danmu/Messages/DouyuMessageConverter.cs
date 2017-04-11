using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DouyuDanmu.Messages
{
    public abstract class DouyuMessageConverter<TDouyuMessage>
    {
        public abstract TDouyuMessage ParseString(string douyuMessage);

        public abstract string DumpsString(TDouyuMessage douyuMessage);
    }
}
