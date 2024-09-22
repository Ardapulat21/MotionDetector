﻿using ImageProcessing.Interfaces;
using ImageProcessing.Models;
using ImageProcessing.Services.Buffers;
using ImageProcessing.Services.IO;
using ImageProcessing.Services.MotionDetection;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Buffer = ImageProcessing.Services.Buffers.Buffer;

namespace ImageProcessing.Services.VideoProcessing
{
    public class Processor : IProcessor , IRunner
    {
        private static Buffer _nextBuffer;
        private static MotionDetector _motionDetector;
        public Task Task;
        public async Task Run()
        {
            Task = new Task(_processor.Process);
            Task.Start();
        }
        public void Process() 
        {
            State.ProcessingProcess = Enum.ProcessingProcess.Processing;

            while (State.ProcessedFrameIndex < Metadata.NumberOfFrames)
            {
                if (State.ProcessedFrameIndex >= State.DecodedFrameIndex)
                {
                    ConsoleService.WriteLine("Processor is waiting",IO.Color.Yellow);
                    Thread.Sleep(100);
                    continue;
                }
                try
                {
                    var frame = _nextBuffer.ElementAt(State.ProcessedFrameIndex);
                    var BitmapArray = frame;

                    using (MemoryStream ms = new MemoryStream(BitmapArray))
                    {
                        Bitmap bitmap = new Bitmap(ms);
                        _motionDetector.ProcessFrame(bitmap);
                        bitmap.Save(ms, ImageFormat.Bmp);
                        bitmap.Dispose();
                        _nextBuffer.Update(State.ProcessedFrameIndex, ms.ToArray());
                        GC.Collect();
                    }
                    State.ProcessedFrameIndex++;
                }
                catch { }
            }
            State.ProcessingProcess = Enum.ProcessingProcess.Done;
            ConsoleService.WriteLine("Processing has done.",IO.Color.Red);
            return;
        }
        #region Singleton
        private static Processor _processor;
        private Processor()
        {
            _nextBuffer = NextBuffer.GetInstance();
            _motionDetector = new MotionDetector();
        }
        public static Processor GetInstance()
        {
            if (_processor == null)
                _processor = new Processor();

            return _processor;
        }
        #endregion
    }
}
