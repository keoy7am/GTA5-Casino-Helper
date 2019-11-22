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
using keyTypes = Process.NET.Native.Types.Keys;

namespace GTA5_Casino_Helper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        // TODO Log
        // TODO Implement Russian Roulette Mode
        // TODO Hotkey
        SysProcess _process { get; set; }
        ProcessSharp _sharp { get; set; }
        bool isHB_Running = false;
        bool isBR_Running = false;
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void StatusBar_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                _process = SysProcess.GetProcessesByName("GTA5").First();
                if (_process != null)
                {
                    _sharp = new ProcessSharp(_process, Process.NET.Memory.MemoryType.Remote);
                    //ProcessSharp.Memory = new ExternalProcessMemory(sharp.Handle);
                    await SetUIAsync(true);
                }
            }
            catch (Exception ex)
            {

            }
        }
        private async void btn_EarnMoneyByHB_Click(object sender, RoutedEventArgs e)
        {
            if (isBR_Running)
            {
                MessageBox.Show("You are running russian roulette now.");
                return;
            }
            await HR_ClickBet();
        }
        private void btn_EarnMoneyByRR_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Non-Support Now.");
            if (isHB_Running)
            {
                MessageBox.Show("You are running horse betting now.");
                return;
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
                        btn_EarnMoneyByBR.IsEnabled = true;
                    }));
                }
                else
                {
                    await this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        StatusBar.Text = "Status：Game Detected!";
                        StatusBar.IsEnabled = true;
                        btn_EarnMoneyByHB.IsEnabled = false;
                        btn_EarnMoneyByBR.IsEnabled = false;
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
            if (isHB_Running)
            {
                await SetStatus("關閉自動下注");
                isHB_Running = false;
            }
            else
            {
                await SetStatus("啟用自動下注");
                isHB_Running = true;
            }
            while (isHB_Running)
            {
                // TODO 寫成Action
                await SetStatus("執行自動下注");
                window.Activate();
                window.Mouse.MoveTo(1450, 920);
                await Task.Delay(1000);
                SimulateHelper.LeftClick(1450, 920);

                await SetStatus("選擇賽馬");
                window.Mouse.MoveTo(330, 350);
                await Task.Delay(1000);
                SimulateHelper.LeftClick(330, 350);

                await SetStatus("設定金額");
                window.Mouse.MoveTo(1530, 520);
                await Task.Delay(1000);
                for (var i = 10; i > 0; i--)
                {
                    await Task.Delay(50);
                    SimulateHelper.LeftClick(1530, 520);
                }

                await SetStatus("執行下注");
                await Task.Delay(250);
                SimulateHelper.PressLeft(1530, 520);

                await SetStatus("等待賽事完成");
                await Task.Delay(250);
                window.Mouse.MoveTo(1300, 800);
                await Task.Delay(35000);
                SimulateHelper.ReleaseLeft(1300, 800);

                await SetStatus("結束賽事");
                await Task.Delay(50);
                window.Mouse.MoveTo(950, 1010);
                SimulateHelper.LeftClick(950, 1010);

                for(int i = 500; i > 0; i--)
                {
                    await Task.Delay(1);
                    await SetStatus($"將在 {i/100} 秒後重新執行下注");
                    if (!isHB_Running)
                    {
                        await SetStatus($"結束。");
                        return;
                    }
                }
            }
        }
    }
}
