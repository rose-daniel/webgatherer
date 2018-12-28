using System;

namespace vip.common
{
    public class PagerInfo
    {
        public int prePageIndex { set; get; }
        public int nextPageIndex { set; get; }
        public double beginPageIndex { set; get; }
        public double endPageIndex { set; get; }
        public int pageTotal { set; get; }
        public int currPageIndex { set; get; }
        public int recordCount { set; get; }
    }
}
