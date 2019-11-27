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
using NLog;
using System.IO;
using System.Reflection;

namespace GTA5_Casino_Helper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        // TODO RRMode: Supports detection of Casinocoin changes. (To make sure a game has been lost)
        private static Logger logger = NLog.LogManager.GetCurrentClassLogger();
        SysProcess _process { get; set; }
        ProcessSharp _sharp { get; set; }
        Thread RRWorkerThread;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            RRWorkerThread = new Thread(RRWorker);
            RRWorkerThread.IsBackground = true;
            RRWorkerThread.Start();

            cb_RR_Number.ItemsSource = AppData.shared.RR_NumberList;
            cb_RR_Number.SelectedIndex = 0;

            cb_RR_Amount.ItemsSource = AppData.shared.RR_AmountList;
            cb_RR_Amount.SelectedItem = AppData.shared.RR_AmountList.LastOrDefault();

            HotkeyManager.Current.AddOrReplace(Key.NumPad1.ToString(), Key.NumPad1, ModifierKeys.Control, DetectGameHotkeyEvent);
            HotkeyManager.Current.AddOrReplace(Key.NumPad2.ToString(), Key.NumPad2, ModifierKeys.Control, SwitchHBModeHotkeyEvent);
            HotkeyManager.Current.AddOrReplace(Key.NumPad3.ToString(), Key.NumPad3, ModifierKeys.Control, SwitchRRModeHotkeyEvent);
            HotkeyManager.Current.AddOrReplace(Key.NumPad9.ToString(), Key.NumPad9, ModifierKeys.Control, CloseAppHotkeyEvent);

            var version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion.ToString();
            this.Title = $"GTA 5 Casino Helper ( Quit: Ctrl + NumPad9 ) | ver.{version}";
            logger.Info("Program Start!");
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
                    logger.Info("已偵測到 GTA5.exe 。");
                }
            }
            catch (Exception ex)
            {
                logger.Fatal($"DetectGame Failed:{ex.Message}");
            }
        }
        private async Task SwitchHBMode()
        {
            try
            {
                if (_process == null)
                {
                    await SetStatus("You must detect game first (Ctrl+ NumPad1).");
                    return;
                }

                if (AppData.shared.isRR_Running)
                {
                    await SetStatus("You are running russian roulette now.");
                    return;
                }


                if (AppData.shared.isHB_Running)
                {
                    logger.Info("關閉自動下注。");
                    await SetUITopMust(false);
                    await SetStatus("結束自動下注");
                    AppData.shared.isHB_Running = false;
                }
                else
                {
                    logger.Info("啟用自動下注。");
                    await SetUITopMust(true);
                    await SetStatus("啟用自動下注");
                    AppData.shared.isHB_Running = true;
                }
                await HR_AutoBetScript();
            }
            catch (Exception ex)
            {
                logger.Fatal($"SwitchHBMode Failed:{ex.Message}");
            }
        }
        private async Task SwitchRRMode()
        {
            try
            {
                if (_process == null)
                    return;

                if (AppData.shared.isHB_Running)
                {
                    MessageBox.Show("You are running horse betting now.");
                    return;
                }


                if (AppData.shared.isRR_Running)
                {
                    logger.Info("結束俄羅斯輪盤修改。");
                    await SetUITopMust(false);
                    await SetStatus("已關閉俄羅斯輪盤修改,等待程式停止..");
                    await Task.Delay(2000);
                    await SetStatus("結束俄羅斯輪盤修改");
                    AppData.shared.isRR_Running = false;
                    AppData.shared.RR_Amount_TryTimes = 0;
                    AppData.shared.RR_Number_TryTimes = 0;
                }
                else
                {
                    logger.Info("啟用俄羅斯輪盤修改。");
                    await SetUITopMust(true);
                    await SetStatus("已啟用俄羅斯輪盤修改,等待程式執行..");
                    AppData.shared.isRR_Running = true;
                }
            }
            catch (Exception ex)
            {
                logger.Fatal($"SwitchRRMode Failed:{ex.Message}");
            }
        }
        private void CloseApp()
        {
            logger.Info("程式結束。");
            this.Close();
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

        private void CloseAppHotkeyEvent(object sender, HotkeyEventArgs e)
        {
            CloseApp();
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
            AppData.shared.RR_Number = cb_RR_Number.SelectedIndex;
        }

        private void cb_RR_Amount_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AppData.shared.RR_Amount = AppData.shared.RR_AmountList[cb_RR_Amount.SelectedIndex];
        }
        #endregion
        #region UI Update Methods
        private async Task SetUITopMust(bool enable)
        {
            // TODO Config
            try
            {

                if (enable)
                {
                    await this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        this.Topmost = true;
                        this.Opacity = 0.7;
                        this.Top = 120;
                        this.Left = 0;
                        this.Background = new SolidColorBrush(Color.FromArgb(0, 0x1F, 0x55, 0));
                    }));
                }
                else
                {
                    await this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        this.Topmost = false;
                        this.Opacity = 1;
                        this.Top = 120;
                        this.Left = 0;
                        this.Background = SystemColors.ControlBrush;
                    }));
                }
            }
            catch(Exception ex)
            {
                logger.Fatal($"SetUITopMust Failed:{ex.Message}");
            }
        }

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
                logger.Fatal($"SetUIAsync Failed:{ex.Message}");
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
                logger.Fatal($"SetStatus Failed:{ex.Message}");
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
                    if (!AppData.shared.isRR_Running)
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(5));
                        continue;
                    }

                    if (await SetBettingAmountAsync() && await SetBettingNumberAsync())
                        await SetStatus($"已鎖定俄羅斯輪盤出 {AppData.shared.RR_Number} , 金額為 {AppData.shared.RR_Amount}。");
                }
                catch (Exception ex)
                {
                    await SetStatus($"RRWorker() => {ex.Message}");
                    logger.Fatal($"RRWorker Error:{ex.Message}");
                    AppData.shared.isRR_Running = false;
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
                if (AppData.shared.RR_Amount_TryTimes > 10)
                {
                    // TODO Alert: Please try to connect to new session. The hack is not working here in this session. ( but it should be work in all of the session)
                    await SwitchRRMode();
                    return false;
                }
                IntPtr bettingAmountPtr = MemoryHelper.GetPtr(_process, AppData.shared.RR_BettingAmout_Offsets, true);
                IntPtr bettingAmountPtr2 = IntPtr.Add(bettingAmountPtr, 16);
                IntPtr bettingAmountPtr3 = IntPtr.Add(bettingAmountPtr2, 16);
                IntPtr bettingAmountPtr4 = IntPtr.Add(bettingAmountPtr3, 16);
                byte[] bettingAmount = BitConverter.GetBytes(AppData.shared.RR_Amount);

                _sharp.Memory = new ExternalProcessMemory(_sharp.Handle);
                _sharp.Memory.Write((IntPtr)bettingAmountPtr, bettingAmount);
                _sharp.Memory.Write((IntPtr)bettingAmountPtr2, bettingAmount);
                _sharp.Memory.Write((IntPtr)bettingAmountPtr3, bettingAmount);
                _sharp.Memory.Write((IntPtr)bettingAmountPtr4, bettingAmount);

                AppData.shared.RR_Amount_TryTimes = 0;
            }
            catch (Exception ex)
            {
                await SetStatus($"SetBettingAmount():{ex.Message}");
                logger.Fatal($"SetBettingAmount Failed:{ex.Message}");
                AppData.shared.RR_Amount_TryTimes++;
                return false;
            }
            return true;
        }

        private async Task<bool> SetBettingNumberAsync()
        {
            try
            {
                if (AppData.shared.RR_Number_TryTimes > 10)
                {
                    // TODO Alert: Please try to connect to new session. The hack is not working here in this session.
                    await SwitchRRMode();
                    AppData.shared.RR_Number_TryTimes = 0;
                    return false;
                }
                var pointerList = AppData.shared.GetRR_BettingNumber_OffsetList();
                byte[] bettingNumber = BitConverter.GetBytes(AppData.shared.RR_Number);
                _sharp.Memory = new ExternalProcessMemory(_sharp.Handle);

                foreach (var pointer in pointerList)
                {
                    IntPtr numberBase = MemoryHelper.GetPtr(_process, pointer, true);
                    IntPtr table1 = IntPtr.Add(numberBase, 8);
                    IntPtr table2 = IntPtr.Add(table1, 8);

                    _sharp.Memory.Write(table1, bettingNumber);
                    _sharp.Memory.Write(table2, bettingNumber);
                }

                AppData.shared.RR_Number_TryTimes = 0;
            }
            catch (Exception ex)
            {
                await SetStatus($"SetBettingNumber():{ex.Message}");
                AppData.shared.RR_Number_TryTimes++;
                return false;
            }
            return true;
        }
        #endregion
        #region HR Mode Auto Script
        private async Task HR_AutoBetScript()
        {
            var window = _sharp.WindowFactory.MainWindow;
            while (AppData.shared.isHB_Running)
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
                    if (!AppData.shared.isHB_Running)
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
