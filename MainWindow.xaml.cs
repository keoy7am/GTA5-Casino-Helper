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
using NHotkey.Wpf;
using NHotkey;

namespace GTA5_Casino_Helper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        int RR_Number = 0;
        int RR_Amount = 1000;
        // TODO Log
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
            (IntPtr)0x02889FF0,
            (IntPtr)0x68,
            (IntPtr)0x3D8,
            (IntPtr)0x10,
            (IntPtr)0x108,
            (IntPtr)0x4D0
        };
        Thread RRWorkerThread;
        List<string> RR_NumberList = new List<string>();
        List<int> RR_AmountList = new List<int>()
        {
            10,
            50,
            100,
            500,
            5000,
            50000,
            //60000 //  After testing, the value will have a high risk of freezing in the casino.
        };
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            RRWorkerThread = new Thread(RRWorker);
            RRWorkerThread.IsBackground = true;
            RRWorkerThread.Start();

            for (int i = 0; i <= 36; i++)
            {
                RR_NumberList.Add(i.ToString());
            }
            RR_NumberList.Add("00");

            cb_RR_Number.ItemsSource = RR_NumberList;
            cb_RR_Number.SelectedIndex = 0;

            cb_RR_Amount.ItemsSource = RR_AmountList;
            cb_RR_Amount.SelectedIndex = 0;
            HotkeyManager.Current.AddOrReplace("F1", Key.F1, ModifierKeys.Control, DetectGameHotkeyEvent);
            HotkeyManager.Current.AddOrReplace("F2", Key.F2, ModifierKeys.Control, SwitchHBModeHotkeyEvent);
            HotkeyManager.Current.AddOrReplace("F3", Key.F3, ModifierKeys.Control, SwitchRRModeHotkeyEvent);
        }
        #region Program Contorl
        private async Task DetectGame()
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
        private async Task SwitchHBMode()
        {
            try
            {
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
                await HR_AutoBetScript();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}");
            }
        }
        private async Task SwitchRRMode()
        {
            try
            {
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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}");
            }
        }
        #endregion
        #region HotEvents
        private async void DetectGameHotkeyEvent(object sender, HotkeyEventArgs e)
        {
            await DetectGame();
        }

        private async void SwitchHBModeHotkeyEvent(object sender, HotkeyEventArgs e)
        {
            await SwitchHBMode();
        }

        private async void SwitchRRModeHotkeyEvent(object sender, HotkeyEventArgs e)
        {
            await SwitchRRMode();
        }
        #endregion
        #region UI Event
        private async void StatusBar_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            await DetectGame();
        }

        private async void btn_EarnMoneyByHB_Click(object sender, RoutedEventArgs e)
        {
            await SwitchHBMode();
        }

        private async void btn_EarnMoneyByRR_Click(object sender, RoutedEventArgs e)
        {
            await SwitchRRMode();
        }

        private void cb_RR_Number_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RR_Number = cb_RR_Number.SelectedIndex;
        }

        private void cb_RR_Amount_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RR_Amount = RR_AmountList[cb_RR_Amount.SelectedIndex];
        }
        #endregion
        #region UI Update Methods
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
        #endregion
        #region RR Mode Memory Hacking
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

                    if (await SetBettingAmountAsync() && await SetBettingNumberAsync())
                        await SetStatus($"已鎖定俄羅斯輪盤出 {RR_Number} , 下 {RR_Number} 金額為 {RR_Amount}。");
                }
                catch (Exception ex)
                {
                    await SetStatus($"RRWorker() => {ex.Message}");
                    isRR_Running = false;
                }
                finally
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(250));
                }
            }
        }
        private async Task<bool> SetBettingAmountAsync()
        {
            try
            {
                IntPtr bettingAmountPtr = MemoryHelper.GetPtr(_process, RR_BettingAmout_Offsets, true);
                IntPtr bettingAmountPtr2 = IntPtr.Add(bettingAmountPtr, 16);
                IntPtr bettingAmountPtr3 = IntPtr.Add(bettingAmountPtr2, 16);
                IntPtr bettingAmountPtr4 = IntPtr.Add(bettingAmountPtr3, 16);
                byte[] bettingAmount = BitConverter.GetBytes(RR_Amount);

                _sharp.Memory = new ExternalProcessMemory(_sharp.Handle);
                _sharp.Memory.Write((IntPtr)bettingAmountPtr, bettingAmount);
                _sharp.Memory.Write((IntPtr)bettingAmountPtr2, bettingAmount);
                _sharp.Memory.Write((IntPtr)bettingAmountPtr3, bettingAmount);
                _sharp.Memory.Write((IntPtr)bettingAmountPtr4, bettingAmount);
            }
            catch (Exception ex)
            {
                await SetStatus($"SetBettingAmount():{ex.Message}");
                return false;
            }
            return true;
        }
        private async Task<bool> SetBettingNumberAsync()
        {
            try
            {
                IntPtr bettingNumberPtr = MemoryHelper.GetPtr(_process, RR_BettingNumber_Offsets, true);
                IntPtr bettingNumberPtr2 = IntPtr.Add(bettingNumberPtr, 8);
                byte[] bettingNumber = BitConverter.GetBytes(RR_Number);

                _sharp.Memory = new ExternalProcessMemory(_sharp.Handle);
                _sharp.Memory.Write((IntPtr)bettingNumberPtr, bettingNumber);
                _sharp.Memory.Write((IntPtr)bettingNumberPtr2, bettingNumber);
            }
            catch (Exception ex)
            {
                await SetStatus($"SetBettingNumber():{ex.Message}");
                return false;
            }
            return true;
        }
        #endregion
        #region HR Mode Auto Script
        private async Task HR_AutoBetScript()
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
        #endregion
    }
}
