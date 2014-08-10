using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DevExpress.Xpf.Ribbon;
using Microsoft.Win32;
using System.Windows.Shapes;
using System.Text;
using System.IO;
using System.Reflection;

namespace Mapeditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : DXRibbonWindow
    {
        private Rectangle _currentRecObj = new Rectangle();
        private Rectangle _clickRecObj;
        private Image _currentImg = new Image();
        private Image _clickImg;
        private ImageInfo _imgInfo;
        private RectangleInfo _recInfo;
        private String _currentBGPath = string.Empty;
        private double _currPosX;  // Current position X
        private double _currPosY;  // Current position Y
        private bool _isDraw = true; // is draw
        private bool _isMove = false; // is move
        private bool _isRecObj = false; //is rectangle object

        public MainWindow()
        {
            InitializeComponent();
        }

        /**
         * 
         * Description:
         *  + Phuong thuc nay dung de load anh back ground len canvas
         * 
         */

        private void btnLoadBG_ItemClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            var openFile = new OpenFileDialog() { Title = "Open Background", Multiselect = false, Filter = "Image file (.png) |*.png|All files (*.*)|*.*" };
            var _Result = openFile.ShowDialog();
            if (_Result == true)
            {
                if (openFile.CheckFileExists)
                {
                    _currentBGPath = openFile.FileName;
                    var img = new ImageBrush() { ImageSource = new BitmapImage(new Uri(_currentBGPath, UriKind.RelativeOrAbsolute)), Stretch = Stretch.UniformToFill};
                    cvMap.Background = img;
                    cvMap.Height = img.ImageSource.Height;
                    cvMap.Width = img.ImageSource.Width;
                }
            }
        }

        /**
         * Description:
         *      + get position of mouse
         * @Param sender
         * @Param e
         */ 
        private void cvMap_MouseMove(object sender, MouseEventArgs e)
        {
            _currPosX = e.GetPosition(cvMap).X; //set value of current position X
            _currPosY = e.GetPosition(cvMap).Y; //set value of current position X
            txtPosX.Text = _currPosX.ToString();
            txtPosY.Text = _currPosY.ToString();
            //move image
            if (_isMove)
            {
                if (cvMap.Children.Contains(_currentImg))
                {
                    cvMap.Children.Remove(_currentImg);
                }
                if (_isRecObj)
                {
                    this.drawRectangleToCanvas(_clickRecObj, _currPosX, _currPosY);
                    //this._isMove = false;
                    this._isDraw = true;
                }
                else
                {
                    this.drawImageToCanvas(_clickImg, _currPosX, _currPosY);
                    //this._isMove = false;
                    this._isDraw = true;
                }
            }
            else
            {
                //draw image with possition of mouse
                if (_currentImg.Source != null && !_isRecObj)
                {
                    //if canvas contain object image then move positon
                    if (cvMap.Children.Contains(_currentImg))
                    {
                        Canvas.SetLeft(_currentImg, _currPosX + 5);
                        Canvas.SetTop(_currentImg, _currPosY + 5);
                    }
                    else
                    {
                        cvMap.Children.Add(_currentImg);
                        Canvas.SetLeft(_currentImg, _currPosX + 5);
                        Canvas.SetTop(_currentImg, _currPosY + 5);
                    }
                }
            }
        }

        /**
         * @Param: sender
         * @Param: e
         * Desciption:
         *  + Get Image source
         * return
         */
        private void imageItem_Click(object sender, EventArgs e)
        {
            var glItem = sender as DevExpress.Xpf.Bars.GalleryItem;
            if (glItem.Tag != null)
            {
                _currentImg.Source = glItem.Glyph;
                _currentImg.Tag = glItem.Tag;
                _isRecObj = false;
            }
            
        }

        /**
         * Description:
         *      + draw image to canvas at posX, posY
         * @Param: image
         * @Param: posX
         * @Param: posY
         * Return 
         */ 
        private void drawImageToCanvas(Image image, double posX, double posY)
        {
            if (image != null)
            {
                //if canvas contain object image then move positon
                if (cvMap.Children.Contains(image))
                {
                    //create info of image
                    _imgInfo = image.Tag as ImageInfo;
                    string imgID = _imgInfo.imgID;
                    double posCenterX = posX + image.Source.Width / 2;
                    double posCenterY = cvMap.Height - posY - image.Source.Height / 2;
                    _imgInfo = new ImageInfo(imgID, posCenterX, posCenterY, image.Source.Height, image.Source.Width);
                    image.Tag = _imgInfo;

                    //update posotion
                    Canvas.SetLeft(image, posX);
                    Canvas.SetTop(image, posY);
                }
                else
                {
                    string imgID = image.Tag.ToString();
                    double posCenterX = posX + image.Source.Width / 2;
                    double posCenterY = cvMap.Height - posY - image.Source.Height / 2;
                    _imgInfo = new ImageInfo(imgID, posCenterX, posCenterY, image.Source.Height, image.Source.Width);
                    image.Tag = _imgInfo;
                    
                    //
                    cvMap.Children.Add(image);
                    Canvas.SetLeft(image, posX);
                    Canvas.SetTop(image, posY);
                }
            }
        }

        /**
         * Description:
         *  + create image
         * 
         */
        private Image createImage(ImageSource imgSource)
        {
            if (imgSource != null)
            {
                // create image
                Image img = new Image() { Source = imgSource };
                img.MouseLeftButtonDown += new MouseButtonEventHandler(img_MouseLeftButtonDown);
                img.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(img_PreviewMouseLeftButtonUp);
                img.MouseRightButtonDown += new MouseButtonEventHandler(img_MouseRightButtonDown);
                return img;
            }
            return null;
        }

        /**
         * Description:
         *  + Delete image from canvas
         */ 
        void img_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Image img = sender as Image;
            if(cvMap.Children.Contains(img))
            {
                cvMap.Children.Remove(img);
            }
        }

        void img_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this._isMove = false;
            this._isDraw = true;
        }

        void img_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this._isMove = true;
            this._isDraw = false;
            this._isRecObj = false;
            this._clickImg = sender as Image;
        }

        /**
         * Desciption:
         *      + draw image current to canvas
         */ 
        private void cvMap_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_isDraw)
            {
                if (_isRecObj)
                {
                    if (_currentRecObj != null)
                    {

                        // create image info 
                        if (_currentRecObj.Tag != null) // Check id of image
                        {
                            // create image
                            Rectangle rec = this.createRec(_currentRecObj.Tag.ToString());
                            this.drawRectangleToCanvas(rec, _currPosX, _currPosY);
                        }

                    }
                }
                else
                {
                    if (_currentImg != null)
                    {
                        // create image
                        Image img = this.createImage(_currentImg.Source);
                        // create image info 
                        if (_currentImg.Tag != null) // Check id of image
                        {
                            img.Tag = _currentImg.Tag;
                            this.drawImageToCanvas(img, _currPosX, _currPosY);
                        }

                    }
                }
            }
        }

        /**
         * Description:
         *    + Create new map
         * 
         */
        private void newItem_ItemClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            this.cvMap.Children.Clear(); //remove all object
            this.cvMap.Height = this.cvMap.Width = 0;
        }

        /**
         * 
         * 
         */
        private void saveAsItem_ItemClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            SaveFileDialog saveDlg = new SaveFileDialog() { DefaultExt = ".csv",
                                                            Title = "Save Map", 
                                                            Filter = "Data document (.csv) |*.csv |All files (*.*)|*.*" };
            Nullable<bool> _Result = saveDlg.ShowDialog();
            if (_Result == true)
            {
                if (saveDlg.CheckPathExists)
                {
                    string filePath = saveDlg.FileName;
                    writeObjectToFile(filePath);
                    writeQuadTreeToFile(String.Format("{0}quadTree.csv", filePath.Substring(0, filePath.Length - 5)));
                }
            }
        }

        /**
         * 
         * 
         * 
         */
        private void writeObjectToFile(string filePath)
        {
            int count = 0;
            //Delete current mock image
            if (cvMap.Children.Contains(_currentImg))
            {
                cvMap.Children.Remove(_currentImg);
            }

            System.IO.TextWriter writeFile;
            if (!System.IO.File.Exists(filePath))
            {
                writeFile = new System.IO.StreamWriter(filePath, true);
            }
            else
            {
                System.IO.File.Delete(filePath);
                writeFile = new System.IO.StreamWriter(filePath, false);
            }

            int size = cvMap.Children.Count;
            Image img;
            ImageInfo imgInfo;
            Rectangle rec;
            RectangleInfo recInfo;

            for (int i = 0; i < size; i++)
            {
                if (cvMap.Children[i] is Image)
                { 
                    img = cvMap.Children[i] as Image;
                    imgInfo = img.Tag as ImageInfo;
                    writeFile.WriteLine(String.Format("{0}\t{1}\t{2}\t{3}]\t{4}\t{5}",new Object[]{ count,
                                                                                          imgInfo.imgID, 
                                                                                          (int)imgInfo.posX,
                                                                                          (int)imgInfo.posY, 
                                                                                          (int)imgInfo.height, 
                                                                                          (int)imgInfo.width }));
                    count++;
                }
                else if (cvMap.Children[i] is Rectangle)
                {
                    rec = cvMap.Children[i] as Rectangle;
                    recInfo = rec.Tag as RectangleInfo;
                    writeFile.WriteLine(String.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", new Object[]{ count,
                                                                                          recInfo.recID, 
                                                                                          (int)recInfo.posX,
                                                                                          (int)recInfo.posY, 
                                                                                          (int)recInfo.height, 
                                                                                          (int)recInfo.width }));
                    count++;
                }
            }
            writeFile.Close();
            writeFile.Dispose();
        }

        /**
         * Description:
         *      + draw rectangle to canvas at posX, posY
         * @Param: rectangle
         * @Param: posX
         * @Param: posY
         * Return 
         */
        private void drawRectangleToCanvas(Rectangle rec, double posX, double posY)
        {
            if (rec != null)
            {
                //if canvas contain object image then move positon
                if (cvMap.Children.Contains(rec))
                {
                    //create info of image
                    _recInfo = rec.Tag as RectangleInfo;
                    string recID = _recInfo.recID;
                    double posCenterX = posX + rec.Width / 2;
                    double posCenterY = cvMap.Height - posY - rec.Height / 2;
                    _recInfo = new RectangleInfo(recID, posCenterX, posCenterY, rec.Height, rec.Width);
                    rec.Tag = _recInfo;

                    //update posotion
                    Canvas.SetLeft(rec, posX);
                    Canvas.SetTop(rec, posY);
                }
                else
                {
                    string recID = rec.Tag.ToString();
                    double posCenterX = posX + rec.Width / 2;
                    double posCenterY = cvMap.Height - posY - rec.Height / 2;
                    _recInfo = new RectangleInfo(recID, posCenterX, posCenterY, rec.Height, rec.Width);
                    rec.Tag = _recInfo;

                    //
                    cvMap.Children.Add(rec);
                    Canvas.SetLeft(rec, posX);
                    Canvas.SetTop(rec, posY);
                }
            }
        }

        /**
         * Description:
         *  + create rectangle
         * 
         */
        private int _temp = 16;
        private int _tileSize = 32;
        private Rectangle createRec(String recID)
        {
            if (!String.IsNullOrEmpty(recID))
            {
                // create image
                Rectangle rec = new Rectangle() { Height = _tileSize,
                                                  Width = _tileSize, 
                                                  Tag = recID, 
                                                  Stroke = Brushes.White, 
                                                  Fill = new SolidColorBrush(), 
                                                  StrokeThickness = 2 };
                rec.MouseLeftButtonDown += new MouseButtonEventHandler(rec_MouseLeftButtonDown);
                rec.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(rec_PreviewMouseLeftButtonUp);
                rec.MouseRightButtonDown += new MouseButtonEventHandler(rec_MouseRightButtonDown);
                rec.MouseMove += new MouseEventHandler(rec_MouseMove);
                return rec;
            }
            return null;
        }

        void rec_MouseMove(object sender, MouseEventArgs e)
        {
            Rectangle box = new Rectangle();
            Rectangle rec = sender as Rectangle;
        }

        void rec_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Rectangle rec = sender as Rectangle;
            if (cvMap.Children.Contains(rec))
            {
                cvMap.Children.Remove(rec);
            }
        }

        void rec_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this._isMove = false;
            this._isDraw = true;
            //this._isRecObj = false;
            
        }

        void rec_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this._isMove = true;
            this._isDraw = false;
            this._isRecObj = true;
            this._clickRecObj = sender as Rectangle;

        }

        private void HideObjectItem_Click(object sender, EventArgs e)
        {
            var glItem = sender as DevExpress.Xpf.Bars.GalleryItem;
            if (glItem.Tag != null)
            {
                _isRecObj = true;
                _currentRecObj.Tag = glItem.Tag;
            }
        }

        /**
         * Description:
         *  + create info of image 
         * 
         */ 
        class ImageInfo
        {
            public string imgID;
            public double posX;
            public double posY;
            public double height;
            public double width;

            public ImageInfo(string imgID, double posX, double posY, double height, double width)
            {
                this.imgID = imgID;
                this.posX = posX;
                this.posY = posY;
                this.height = height;
                this.width = width;
            }

            public ImageInfo()
            {

            }
        };

        /**
         * Description:
         *  + create info of rectangle 
         * 
         */ 
        class RectangleInfo
        {
            public string recID;
            public double posX;
            public double posY;
            public double height;
            public double width;

            public RectangleInfo(string recID, double posX, double posY, double height, double width)
            {
                this.recID = recID;
                this.posX = posX;
                this.posY = posY;
                this.height = height;
                this.width = width;
            }

            public RectangleInfo()
            {

            }
        };

        private void DXRibbonWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (this._clickRecObj != null)
            {
                int posY = (int)Canvas.GetTop(this._clickRecObj);
                int posX = (int)Canvas.GetLeft(this._clickRecObj);
                if (e.Key == Key.Left)
                {
                    if (this._clickRecObj.Width > _temp)
                    {
                        this._clickRecObj.Width -= _temp;
                    }
                }
                else if (e.Key == Key.Right)
                {
                    if (posX + this._clickRecObj.Width < this.cvMap.Width)
                    {
                        this._clickRecObj.Width += _temp;
                    }
                }
                else if (e.Key == Key.Up)
                {
                    if (this._clickRecObj.Height > _temp)
                    {
                        this._clickRecObj.Height -= _temp;
                    }
                }
                else if (e.Key == Key.Down)
                {
                    if (posY + this._clickRecObj.Height < (int)this.cvMap.Height)        
                    {
                        this._clickRecObj.Height += _temp;
                    }
                }

                _recInfo = _clickRecObj.Tag as RectangleInfo;
                string recID = _recInfo.recID;
                double posCenterX = posX + _clickRecObj.Width / 2;
                double posCenterY = cvMap.Height - posY - _clickRecObj.Height / 2;
                _recInfo = new RectangleInfo(recID, posCenterX, posCenterY, _clickRecObj.Height, _clickRecObj.Width);
                _clickRecObj.Tag = _recInfo;
            }

        }

        /**
         * Description:
         *  + Create list game object
         * 
         */
        private List<GameObject> getListGameObject()
        {
            if (cvMap.Children.Contains(_currentImg))
            {
                cvMap.Children.Remove(_currentImg);
            }
            int count = 0;
            int size = cvMap.Children.Count;
            Image img;
            ImageInfo imgInfo;
            Rectangle rec;
            RectangleInfo recInfo;
            GameObject obj;
            List<GameObject> result = new List<GameObject>();
            for (int i = 0; i < size; i++)
            {
                if (cvMap.Children[i] is Image)
                {
                    img = cvMap.Children[i] as Image;
                    imgInfo = img.Tag as ImageInfo;
                    obj = new GameObject(count,
                                         new System.Drawing.Rectangle((int)imgInfo.posX - (int)imgInfo.width / 2,
                                                                     (int)imgInfo.posY + (int)imgInfo.height / 2, 
                                                                     (int)imgInfo.width, 
                                                                     (int)imgInfo.height), 
                                         imgInfo.imgID);
                    result.Add(obj);
                    count++;
                }
                else if (cvMap.Children[i] is Rectangle)
                {
                    rec = cvMap.Children[i] as Rectangle;
                    recInfo = rec.Tag as RectangleInfo;
                    obj = new GameObject(count,
                                         new System.Drawing.Rectangle((int)recInfo.posX - (int)recInfo.width / 2,
                                                                     (int)recInfo.posY + (int)recInfo.height / 2, 
                                                                     (int)recInfo.width, 
                                                                     (int)recInfo.height), 
                                         recInfo.recID);
                    result.Add(obj);
                    count++;
                }
            }
            return result;
        }

        /**
         * Description: 
         *     +  build quatree
         * 
         */
        private QuadTree _quadTreeRoot;
        private List<System.Text.StringBuilder> buildQuadTree()
        {
            double maxSize = (this.cvMap.Height > this.cvMap.Width) ? this.cvMap.Height : this.cvMap.Width; //get max size
            System.Drawing.Rectangle recRoot = new System.Drawing.Rectangle(0, 0, (int)maxSize, (int)maxSize); //create rectangle
            List<GameObject> listObject = this.getListGameObject(); // get list object of mapeditor
            this._quadTreeRoot = new QuadTree(recRoot); // create node root
            this._quadTreeRoot.buildQuadTree(listObject); // build quatree
            List<System.Text.StringBuilder> listQuadNode = new List<System.Text.StringBuilder>();
            this._quadTreeRoot.export(_quadTreeRoot._root, listQuadNode);
            return listQuadNode;
        }

        /**
         * 
         */
        private void writeQuadTreeToFile(string filePath)
        {
            System.IO.TextWriter writeFile;
            if (!System.IO.File.Exists(filePath))
            {
                writeFile = new System.IO.StreamWriter(filePath, true);
            }
            else
            {
                System.IO.File.Delete(filePath);
                writeFile = new System.IO.StreamWriter(filePath, false);
            }

            List<System.Text.StringBuilder> listQuadNode = this.buildQuadTree();
            int size = listQuadNode.Count;

            for (int i = 0; i < size; i++)
            {
                writeFile.WriteLine(listQuadNode[i]); 
                                  
            }
            writeFile.Close();
            writeFile.Dispose();
        }

        /**
         * Description:
         *     + If mouse leave cvMap then delete currentImage
         */
        private void cvMap_MouseLeave(object sender, MouseEventArgs e)
        {
            if (this.cvMap.Children.Contains(_currentImg))
            {
                this.cvMap.Children.Remove(_currentImg);
            }
        }

        private Dictionary<String, String> listImagePath;
        /**
         * 
         * 
         */
        private void getListImagePath()
        {
            String filePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), 
                                                                                     @"File\Config.csv");
            //String filePath = "pack://application:,,,/Mapeditor;component/File/Config.csv";
            listImagePath = new Dictionary<string, string>();
            System.IO.StreamReader readerFile = new System.IO.StreamReader(filePath);
            String data;
            String[] result;
            String imgID;
            String imgPath;
            System.Text.RegularExpressions.Regex rgx = new System.Text.RegularExpressions.Regex("\t",
                                              System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            readerFile.ReadLine();
            while ((data = readerFile.ReadLine()) != null)
            {
                result = rgx.Split(data);
                if (result.Length >= 2)
                {
                    imgID = result[0];
                    imgPath = result[1];
                    listImagePath.Add(imgID, imgPath);
                }
            }
        }


        /**
         * Description:
         *  + Open file
         * 
         */
        private void openItem_ItemClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            
            StringBuilder imagePath = null;
            var openFile = new OpenFileDialog()
            {
                Title = "Open File Map",
                Multiselect = false,
                Filter = "Data file (.csv) |*.csv|All files (*.*)|*.*"
            };
            var _Result = openFile.ShowDialog();
            if (_Result == true)
            {
                if (openFile.CheckFileExists)
                {
                    this.getListImagePath();
                    imagePath = new StringBuilder(openFile.FileName);
                    System.IO.StreamReader readerFile = new System.IO.StreamReader(imagePath.ToString());
                    Image img;
                    Rectangle rec;
                    String data;
                    String[] result;
                    String iD;
                    String imgPath;
                    String imgID;
                    int posCenterX;
                    int posCenterY;
                    int height;
                    int width;
                    System.Text.RegularExpressions.Regex rgx = new System.Text.RegularExpressions.Regex("\t",
                                                      System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    //readerFile.ReadLine();
                    while ((data = readerFile.ReadLine()) != null)
                    {
                        result = rgx.Split(data);
                        if (result.Length >= 4)
                        {
                            iD = result[0];
                            imgID = result[1];
                            if (int.Parse(imgID.Trim()) > 700 && int.Parse(imgID.Trim()) <= 800)
                            {
                                posCenterX = int.Parse(result[2]);
                                posCenterY = int.Parse(result[3]);
                                height = int.Parse(result[4]);
                                width = int.Parse(result[5]);
                                rec = new Rectangle()
                                {
                                    Height = height,
                                    Width = width,
                                    Tag = imgID,
                                    StrokeThickness = 2,
                                    Stroke = Brushes.White,
                                    Fill = new SolidColorBrush()
                                };
                                this.drawRectangleToCanvas(rec, posCenterX - rec.Width / 2, cvMap.Height - (rec.Height / 2 + posCenterY));
                            }
                            else
                            {
                                imgPath = listImagePath[imgID];
                                img = this.createImage(new BitmapImage(new Uri(imgPath, UriKind.RelativeOrAbsolute)));
                                img.Tag = imgID;
                                posCenterX = int.Parse(result[2]);
                                posCenterY = int.Parse(result[3]);
                                this.drawImageToCanvas(img, posCenterX - img.Source.Width / 2, cvMap.Height - (img.Source.Height / 2 + posCenterY));
                            }
                        }
                    }
                }
            }
        }


    }
}
