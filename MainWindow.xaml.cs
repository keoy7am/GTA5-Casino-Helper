using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using Process.NET;
using Process.NET.Memory;
using SysProcess = System.Diagnostics.Process;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Threading;

namespace GTA5_Casino_Helper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        // TODO Log
        // TODO Hotkey
        SysProcess _process { get; set; }
        ProcessSharp _sharp { get; set; }
        bool isHB_Running = false;
        bool isRR_Running = false;
        static readonly IntPtr[] RR_BettingAmout_Offsets =
            {
            (IntPtr)0x028AA178,
            (IntPtr)0x28,
            (IntPtr)0x48,
            (IntPtr)0x120,
            (IntPtr)0xA8,
            (IntPtr)0x58,
            (IntPtr)0x108,
            (IntPtr)0x34
        };
        static readonly IntPtr[] RR_BettingNumber_Offsets =
            {
            (IntPtr)0x02E18A88,
            (IntPtr)0x8,
            (IntPtr)0x290,
            (IntPtr)0x18,
            (IntPtr)0x3E0,
            (IntPtr)0xFC8,
            (IntPtr)0x378,
            (IntPtr)0x4D0
        };
        Thread RRWorkerThread;
        public MainWindow()
        {
            InitializeComponent();

            RRWorkerThread = new Thread(RRWorker);
            RRWorkerThread.IsBackground = true;
            RRWorkerThread.Start();
        }
        #region Event
        private async void StatusBar_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                _process = SysProcess.GetProcessesByName("GTA5").First();
                if (_process != null)
                {
                    _sharp = new ProcessSharp(_process, Process.NET.Memory.MemoryType.Remote);
                    await SetUIAsync(true);
                }
            }
            catch (Exception ex)
            {

            }
        }
        private async void btn_EarnMoneyByHB_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                #region oooo
                if (isRR_Running)
                {
                    MessageBox.Show("You are running russian roulette now.");
                    return;
                }


                if (isHB_Running)
                {
                    await SetStatus("關閉自動下注");
                    isHB_Running = false;
                    await SetStatus("結束自動下注");
                }
                else
                {
                    await SetStatus("啟用自動下注");
                    isHB_Running = true;
                }
                #endregion
                await HR_ClickBet();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}");
            }

        }
        private async void btn_EarnMoneyByRR_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                #region oooo
                if (isHB_Running)
                {
                    MessageBox.Show("You are running horse betting now.");
                    return;
                }


                if (isRR_Running)
                {
                    await SetStatus("已關閉俄羅斯輪盤修改,等待程式停止..");
                    isRR_Running = false;
                    await Task.Delay(2000);
                    await SetStatus("結束俄羅斯輪盤修改");
                }
                else
                {
                    await SetStatus("已啟用俄羅斯輪盤修改,等待程式執行..");
                    isRR_Running = true;
                }
                #endregion
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}");
            }
        }
        #endregion
        #region RR
        private async void RRWorker()
        {
            while (true)
            {
                try
                {
                    if (!isRR_Running)
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(5));
                        continue;
                    }

                    SetBettingAmount();
                    SetBettingNumber();
                    await SetStatus("已鎖定俄羅斯輪盤出 0 , 下 0 金額為 50000。");
                }
                catch (Exception ex)
                {
                    await SetStatus($"RRWorker() => {ex.Message}");
                    MessageBox.Show($"自動下注已停止，錯誤如下:\n{ex.Message}");
                    isRR_Running = false;
                }
                finally
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(250));
                }
            }
        }
        private void SetBettingAmount()
        {
            try
            {
                IntPtr bettingAmountPtr = MemoryHelper.GetPtr(_process, RR_BettingAmout_Offsets, true);
                byte[] bettingAmount = BitConverter.GetBytes(50000);

                _sharp.Memory = new ExternalProcessMemory(_sharp.Handle);
                _sharp.Memory.Write((IntPtr)bettingAmountPtr, bettingAmount);
            }
            catch (Exception ex)
            {
                throw new Exception($"SetBettingAmount():{ex.Message}");
            }
        }
        private void SetBettingNumber()
        {
            try
            {
                IntPtr bettingNumberPtr = MemoryHelper.GetPtr(_process, RR_BettingNumber_Offsets, true);
                byte[] bettingNumber = BitConverter.GetBytes(0);

                _sharp.Memory = new ExternalProcessMemory(_sharp.Handle);
                _sharp.Memory.Write((IntPtr)bettingNumberPtr, bettingNumber);
            }
            catch (Exception ex)
            {
                throw new Exception($"SetBettingNumber():{ex.Message}");
            }
        }
        #endregion
        private async Task SetUIAsync(bool enable)
        {
            try
            {
                if (enable)
                {
                    await this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        StatusBar.Text = "Status：Game Detected!";
                        StatusBar.IsEnabled = false;
                        btn_EarnMoneyByHB.IsEnabled = true;
                        btn_EarnMoneyByRR.IsEnabled = true;
                    }));
                }
                else
                {
                    await this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        StatusBar.Text = "Status：Game Detected!";
                        StatusBar.IsEnabled = true;
                        btn_EarnMoneyByHB.IsEnabled = false;
                        btn_EarnMoneyByRR.IsEnabled = false;
                    }));
                }
            }
            catch (Exception ex)
            {

            }
        }
        private async Task SetStatus(string message)
        {
            try
            {
                await this.StatusBar.Dispatcher.BeginInvoke(new Action(() =>
                {
                    StatusBar.Text = $"Status：{message}";
                }));
            }
            catch (Exception ex)
            {

            }
        }
        private async Task HR_ClickBet()
        {
            var window = _sharp.WindowFactory.MainWindow;
            while (isHB_Running)
            {
                // TODO 寫成Action
                await SetStatus("執行自動下注");
                window.Activate(); // it contains ShowWindow method
                SimulateHelper.MoveTo(1450, 920);
                await Task.Delay(1000);
                SimulateHelper.LeftClick(1450, 920);

                await SetStatus("選擇賽馬");
                SimulateHelper.MoveTo(330, 350);
                await Task.Delay(1000);
                SimulateHelper.LeftClick(330, 350);

                await SetStatus("設定金額");
                SimulateHelper.MoveTo(1530, 520);
                await Task.Delay(1500);
                for (var i = 9; i > 0; i--)
                {
                    await Task.Delay(50);
                    SimulateHelper.LeftClick(1530, 520);
                }

                await SetStatus("執行下注");
                await Task.Delay(300);
                SimulateHelper.PressLeft(1530, 520);

                await SetStatus("等待賽事完成");
                await Task.Delay(500);
                SimulateHelper.MoveTo(1300, 800);
                await Task.Delay(35000);
                SimulateHelper.ReleaseLeft(1300, 800);

                await SetStatus("結束賽事");
                await Task.Delay(50);
                SimulateHelper.MoveTo(950, 1010);
                SimulateHelper.LeftClick(950, 1010);

                for (int i = 500; i > 0; i--)
                {
                    await Task.Delay(1);
                    await SetStatus($"將在 {i / 100} 秒後重新執行下注");
                    if (!isHB_Running)
                    {
                        await SetStatus($"已結束自動下注。");
                        return;
                    }
                }
            }
        }
    }
}
