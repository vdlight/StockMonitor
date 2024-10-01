using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StocksMonitor.src
{
    public static class StockMonitorLogger
    {
        private static RichTextBox? output;

        private static ConcurrentQueue<string> messageQueue = new ConcurrentQueue<string>();

        public static void SetOutput(RichTextBox textbox)
        {
            output = textbox;
        }

        public static void WriteMsg(string msg)
        {
            // Queue the message to ensure thread safety
            messageQueue.Enqueue(GetTimeString() + " " + msg);

            // If output is set, make sure it's updated on the UI thread
            if (output != null && output.InvokeRequired)
            {
                output.BeginInvoke(new Action(ProcessQueue));
            }
            else
            {
                ProcessQueue();
            }
        }
        private static void ProcessQueue()
        {
            if (output == null) return;

            while (messageQueue.TryDequeue(out string? msg))
            {
                output.AppendText(msg + Environment.NewLine);
            }
            output.SelectionStart = output.Text.Length; // Move caret to the end
            output.ScrollToCaret(); // Auto-scroll t)
        }

        public static string GetTimeString()
        {
            var time = DateTime.Now;
            var hour = time.Hour < 10 ? "0" + time.Hour.ToString() : time.Hour.ToString();
            var minute = time.Minute < 10 ? "0" + time.Minute.ToString() : time.Minute.ToString();
            var second = time.Second < 10 ? "0" + time.Second.ToString() : time.Second.ToString();

            return $"{hour}:{minute}:{second}";
        }
    }

}
