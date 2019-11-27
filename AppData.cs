using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTA5_Casino_Helper
{
    public class AppData
    {
        public AppData()
        {
            Init_Flags();
            Init_RR_NumberList();
            Init_RR_AmountList();
            Init_RR_BettingAmout_Offsets();
            Init_RR_BettingNumber_OffsetList();
            Init_Values();
        }
        // Thread-safe singleton
        private static Lazy<AppData> _instance = new Lazy<AppData>(() => new AppData());
        public static AppData shared { get { return _instance.Value; } }
        #region public member
        public bool isHB_Running;
        public bool isRR_Running;
        public int RR_Number;
        public int RR_Amount;
        public int RR_Number_TryTimes;
        public int RR_Amount_TryTimes;
        public IntPtr[] RR_BettingAmout_Offsets;
        public List<string> RR_NumberList = new List<string>();
        public List<int> RR_AmountList = new List<int>();
        #endregion
        #region private member
        private List<int[]> RR_BettingNumber_OffsetList = new List<int[]>();
        #endregion
        #region Init
        private void Init_Flags()
        {
            isHB_Running = false;
            isRR_Running = false;
        }
        private void Init_Values()
        {
            // TODO Combine the index with the default selection
            RR_Number = 0;
            RR_Amount = RR_AmountList[0];
        }
        private void Init_RR_NumberList()
        {
            for (int i = 0; i <= 36; i++)
            {
                RR_NumberList.Add(i.ToString());
            }

            RR_NumberList.Add("00");
        }
        private void Init_RR_AmountList()
        {
            RR_AmountList.AddRange(new List<int>()
            {
                10,
                50,
                100,
                500,
                5000,
                50000
                //,60000 //  After testing, the value will have a high risk of freezing in the casino.
            });
        }
        private void Init_RR_BettingAmout_Offsets()
        {
            int pointerBase = 0x028AA178;
            int[] pointerOffsets = { 0x28, 0x48, 0x120, 0xA8, 0x58, 0x108, 0x34 };
            RR_BettingAmout_Offsets = new IntPtr[1 + pointerOffsets.Length];
            RR_BettingAmout_Offsets[0] = (IntPtr)pointerBase;

            for(int idx =0; idx < pointerOffsets.Length; idx++)
            {
                RR_BettingAmout_Offsets[idx + 1] = (IntPtr)pointerOffsets[idx];
            }
        }
        private void Init_RR_BettingNumber_OffsetList()
        {
            RR_BettingNumber_OffsetList.Add(new int[] { 0x01C94938, 0x50, 0x18, 0x18, 0xD8, 0x580, 0x108, 0x4C8 });
        }
        #endregion
        #region public methods
        public List<IntPtr[]> GetRR_BettingNumber_OffsetList()
        {
            return RR_BettingNumber_OffsetList.Select(x => x.Select(value => (IntPtr)value).ToArray()).ToList();
        }
        #endregion
    }
}
