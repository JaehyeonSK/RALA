using CSVisualizer.Classes;
using CSVisualizer.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CSVisualizer.Modules
{
    class GuiHandler : IDrawable
    {
        private MainWindow window;
        private const double AREA_HEIGHT = 150.0;
        private const int VAR_WIDTH = 100;
        private const int VAR_HEIGHT = 30;
        private const int OBJECT_WIDTH = 100;
        private const int OBJECT_HEIGHT = 100;
        private System.Drawing.Point currentPosition = new System.Drawing.Point(0, 0);

        private MemoryFrame heapFrame = null;
        private Dictionary<Guid, MemoryFrame> frameDict = new Dictionary<Guid, MemoryFrame>();
        private Dictionary<MemoryFrame, List<VarArea>> varsInMemory = new Dictionary<MemoryFrame, List<VarArea>>();

        //private TextBlock empTextBlock = null;

        private static GuiHandler instance = null;
        public static GuiHandler Instance
        {
            get
            {
                return instance;
            }
        }

        public GuiHandler(MainWindow window)
        {
            this.window = window;

            frameDict = new Dictionary<Guid, MemoryFrame>();
            varsInMemory = new Dictionary<MemoryFrame, List<VarArea>>();
            instance = this;
        }

        private void UpdateCanvasSize(System.Drawing.Size newSize)
        {
            window.Dispatcher.Invoke(() =>
            {
                window.rootCanvas.Width = Math.Max(window.rootCanvas.ActualWidth, newSize.Width);
                window.rootCanvas.Height = Math.Max(window.rootCanvas.Height, newSize.Height);
            });
        }

        public void CreateStack(Guid guid)
        {
            window.Dispatcher.Invoke(() =>
            {
                var methodInfo = ((MethodInfo)Metadata.FindByGuid(guid));
                var mframe = CreateFrame($"Stack ({methodInfo.ClassName}.{methodInfo.Name}-{guid.Shorten()})");
                frameDict.Add(guid, mframe);
                varsInMemory.Add(mframe, new List<VarArea>());
            });
        }

        public void CreateHeap()
        {
            window.Dispatcher.Invoke(() =>
            {
                heapFrame = CreateFrame("Heap Memory");
                varsInMemory.Add(heapFrame, new List<VarArea>());
            });
        }

        public void DestroyStack(Guid guid)
        {
            window.Dispatcher.Invoke(() =>
            {
                // TODO:: 프레임 내부의 모든 요소 제거
                varsInMemory.Remove(frameDict[guid]);
                frameDict.Remove(guid);
            });
        }

        public void CreateVariable(Guid stackGuid, CSDV_VarInfo varInfo)
        {
            window.Dispatcher.Invoke(() =>
            {
                var targetFrame = frameDict[stackGuid];
                var curPos = (System.Drawing.Point)targetFrame.Tag;

                bool isVarType = varInfo.VarType == CSDV_VarInfo.CSDV_Type.VAR_TYPE;
                string varResult = "";

                if (isVarType)
                    // 값이 없는 값 형식 변수일 경우 ?로 표시
                    varResult = ((string)varInfo.Value == null) ? "?" : (string)varInfo.Value;
                else
                    // 값이 없는 참조 형식 변수일 경우 null로 표시, 아닐 경우 축약형 Guid(앞 8자리) 표시
                    varResult = ((Guid)varInfo.Value == Guid.Empty) ? "null" : ((Guid)varInfo.Value).Shorten();

                // 변수 영역 컨트롤 생성 후 위치 조정
                VarArea varArea = new VarArea(varInfo.Name, varInfo.Type, varResult);
                Canvas.SetLeft(varArea, Canvas.GetLeft(targetFrame) + targetFrame.Margin.Left + curPos.X + 10);
                Canvas.SetTop(varArea, Canvas.GetTop(targetFrame) + targetFrame.Margin.Top + 30);

                // 변수 영역 컨트롤에 변수의 값 혹은 참조하는 객체 guid 부착
                varArea.Tag = varInfo.Value;

                window.rootCanvas.Children.Add(varArea);

                // 다음에 생성될 컨트롤 위치 저장
                curPos.X += (int)varArea.Width + 10;
                targetFrame.Tag = curPos;

                // Heap 메모리에 해당 변수 영역 컨트롤 저장
                varsInMemory[targetFrame].Add(varArea);

                // 부모 캔버스 사이즈 조절
                UpdateCanvasSize(new System.Drawing.Size((int)Canvas.GetLeft(varArea) + (int)varArea.Width, 0));
            });
        }

        public void CreateObject(Guid objGuid, List<CSDV_VarInfo> varInfos)
        {
            window.Dispatcher.Invoke(() =>
            {
                var targetFrame = heapFrame;
                var curPos = (System.Drawing.Point)targetFrame.Tag;

                // 변수 영역 컨트롤 생성 후 위치 조정
                VarArea varArea = new VarArea(objGuid, varInfos);
                Canvas.SetLeft(varArea, Canvas.GetLeft(targetFrame) + targetFrame.Margin.Left + curPos.X + 10);
                Canvas.SetTop(varArea, Canvas.GetTop(targetFrame) + targetFrame.Margin.Top + 30);

                // 변수 영역 컨트롤에 객체 guid 부착
                varArea.Tag = objGuid;

                window.rootCanvas.Children.Add(varArea);

                // 다음에 생성될 컨트롤 위치 저장
                curPos.X += (int)varArea.Width + 10;
                targetFrame.Tag = curPos;

                // Heap 메모리에 해당 변수 영역 컨트롤 저장
                varsInMemory[targetFrame].Add(varArea);

                // 부모 캔버스 사이즈 조절
                UpdateCanvasSize(new System.Drawing.Size((int)Canvas.GetLeft(varArea) + (int)varArea.Width, 0));
            });
        }

        /// <summary>
        /// 변수 혹은 객체가 저장된 stack 혹은 heap의 guid
        /// </summary>
        /// <param name="memoryGuid"></param>
        /// <param name="varInfo"></param>
        public void AssignVariable(Guid varGuid, CSDV_VarInfo varInfo)
        {
            window.Dispatcher.Invoke(() =>
            {
                foreach (var varArea in varsInMemory[heapFrame])
                {
                    if (varArea.Tag.GetType() == typeof(Guid)
                        && ((Guid)varArea.Tag) == varGuid)
                    {
                        varArea.SetContents(varGuid, MemoryManager.Instance.GetObject(varGuid));
                        UpdatePositionMemory(heapFrame);
                        return;
                    }
                }

                foreach (var varArea in varsInMemory[frameDict[Context.CurrentMethodContext]])
                {
                    if (varArea.Name == varInfo.Name)
                    {
                        varArea.SetContents(
                            varInfo.Name,
                            varInfo.Type,
                            varInfo.Value.GetType() == typeof(Guid) ?
                            ((Guid)varInfo.Value).Shorten() : (string)varInfo.Value);
                        UpdatePositionMemory(frameDict[Context.CurrentMethodContext]);
                        return;
                    }
                }

                foreach (var varArea in varsInMemory[frameDict[Context.CurrentObjectContext]])
                {
                    if (varArea.Name == varInfo.Name)
                    {
                        varArea.SetContents(
                            varInfo.Name,
                            varInfo.Type,
                            varInfo.Value.GetType() == typeof(Guid) ?
                            ((Guid)varInfo.Value).Shorten() : (string)varInfo.Value);
                        UpdatePositionMemory(frameDict[Context.CurrentObjectContext]);
                        return;
                    }
                }
            });
        }

        public void AssignReference(Guid objGuid, CSDV_VarInfo varInfo)
        {
            AssignVariable(objGuid, varInfo);
            // var line = CreateLine(from, to);
            // connections.Add(from, to);
        }

        private MemoryFrame CreateFrame(string name)
        {
            MemoryFrame mframe = new MemoryFrame(name);
            mframe.Width = 2000;
            mframe.Height = AREA_HEIGHT;
            mframe.Tag = new System.Drawing.Point(0, 0);
            Canvas.SetLeft(mframe, currentPosition.X);
            Canvas.SetTop(mframe, currentPosition.Y);

            currentPosition.Y += (int)AREA_HEIGHT;

            window.rootCanvas.Children.Add(mframe);

            return mframe;
        }

        private void UpdatePositionMemory(MemoryFrame targetFrame)
        {
            List<VarArea> targetList = null;
            targetList = varsInMemory[targetFrame];

            // X 위치를 기준으로 오름차순 정렬
            targetList.Sort((v1, v2) =>
            {
                if (Canvas.GetLeft(v1) >= Canvas.GetLeft(v2)) return 1;
                else return -1;
            });

            // X 위치를 다시 잡음
            for (int i = 1; i < targetList.Count; i++)
            {
                Canvas.SetLeft(targetList[i], Canvas.GetLeft(targetList[i - 1]) + targetList[i - 1].ActualWidth + 10);
            }


            var curPos = (System.Drawing.Point)targetFrame.Tag;
            var lastVar = targetList[targetList.Count - 1];
            curPos.X = (int)(Canvas.GetLeft(lastVar) + lastVar.ActualWidth + 10);
            targetFrame.Tag = curPos;

            int minWidth = (int)(Canvas.GetLeft(lastVar) + lastVar.ActualWidth + 10);
            UpdateCanvasSize(new System.Drawing.Size(minWidth, 0));
        }

        //private void Tick()
        //{
        //    window.Dispatcher.Invoke(() =>
        //    {
        //        if (empTextBlock != null)
        //        {
        //            empTextBlock.Foreground = System.Windows.Media.Brushes.Black;
        //            empTextBlock = null;
        //        }
        //    });
        //}

        public void Init()
        {
            CreateHeap();
        }

        public void WriteLog(object msg)
        {
            window.Dispatcher.Invoke(() =>
            {
                window.textLog.Text += msg.ToString() + Environment.NewLine;
            });
        }
    }
}
