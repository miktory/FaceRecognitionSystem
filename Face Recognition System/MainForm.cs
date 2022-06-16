
//Multiple face detection and recognition in real time
//Using EmguCV cross platform .Net wrapper to the Intel OpenCV image processing library for C#.Net
//Writed by Sergio Andrés Guitérrez Rojas
//"Serg3ant" for the delveloper comunity
// Sergiogut1805@hotmail.com
//Regards from Bucaramanga-Colombia ;)

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Media;

namespace FaceRec
{
    public partial class FrmPrincipal : Form
    {
        //Declararation of all variables, vectors and haarcascades
        Image<Bgr, Byte> currentFrame;
        Capture grabber;
        HaarCascade face;
        HaarCascade eye;
        MCvFont font = new MCvFont(FONT.CV_FONT_HERSHEY_TRIPLEX, 0.5d, 0.5d);
        Image<Gray, byte> result, TrainedFace = null;
        Image<Gray, byte> gray = null;
        List<Image<Gray, byte>> trainingImages = new List<Image<Gray, byte>>();
        List<string> labels = new List<string>();
        List<string> NamePersons = new List<string>();
        int ContTrain, NumLabels, t;
        string name, names = null;
        public static string[] loadedLabels;
        public static Image pictureBoxImg;
        public static bool isPaused = false;

        public FrmPrincipal()
        {
            InitializeComponent();
            //Load haarcascades for face detection
            face = new HaarCascade("haarcascade_frontalface_default.xml");
            //eye = new HaarCascade("haarcascade_eye.xml");
            
            try
            {
                listBox1.Items.Clear();
                //Load of previus trainned faces and labels for each image
                string Labelsinfo = File.ReadAllText(Application.StartupPath + "/TrainedFaces/TrainedLabels.txt");
                string[] Labels = Labelsinfo.Split('%');
                NumLabels = Convert.ToInt16(Labels[0]);
                ContTrain = NumLabels;
                string LoadFaces;

                for (int tf = 1; tf < NumLabels + 1; tf++)
                {
                    LoadFaces = "face" + tf + ".bmp";
                    trainingImages.Add(new Image<Gray, byte>(Application.StartupPath + "/TrainedFaces/" + LoadFaces));
                    labels.Add(Labels[tf]);
                    if (!listBox1.Items.Contains(Labels[tf]))
                        listBox1.Items.Add(Labels[tf]);
                }
                loadedLabels = Labels;
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.ToString());
                MessageBox.Show("База данных пуста, пополните её.", "Обновление базы данных", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        public void UpdateTrainedFaces()
        {
            try
            {
                listBox1.Items.Clear();
                //Load of previus trainned faces and labels for each image
                string Labelsinfo = File.ReadAllText(Application.StartupPath + "/TrainedFaces/TrainedLabels.txt");
                string[] Labels = Labelsinfo.Split('%');
                NumLabels = Convert.ToInt16(Labels[0]);
                ContTrain = NumLabels;
                string LoadFaces;
                trainingImages = new List<Image<Gray, byte>>();
                labels = new List<string>();
                loadedLabels = Labels;
                for (int tf = 1; tf < NumLabels + 1; tf++)
                {
                    LoadFaces = "face" + tf + ".bmp";
                    trainingImages.Add(new Image<Gray, byte>(Application.StartupPath + "/TrainedFaces/" + LoadFaces));
                    labels.Add(Labels[tf]);
                    if (!listBox1.Items.Contains(Labels[tf]))
                        listBox1.Items.Add(Labels[tf]);

                }
                
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.ToString());
                MessageBox.Show("Ошибка. Возможно, база данных пуста.", "Обновление базы данных", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        private void label1_Click(object sender, EventArgs e)
        {

        }




        private void button1_Click(object sender, EventArgs e)
        {
            //Initialize the capture device
            grabber = new Capture();
            grabber.QueryFrame();
            //Initialize the FrameGraber event
            Application.Idle += new EventHandler(FrameGrabber);
            button1.Enabled = false;
            button9.Enabled = true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            StreamWriter sw = new StreamWriter(Application.StartupPath + "/TrainedFaces/" + name + "/info.txt");
            var fullName = label9.Text.ToString();
            var role = label10.Text.ToString();
            var status = "Вход разрешён";
            var additionalInfo = label12.Text.ToString();
            var information = new List<string>();
            information.Add(fullName);
            information.Add(role);
            information.Add(status);
            information.Add(additionalInfo);
            for (int i = 1; i < information.ToArray().Length + 1; i++)
            {
                sw.WriteLine(information.ToArray()[i - 1]);
            }
            sw.Close();
            grabber.QueryFrame();
            Application.Idle += new EventHandler(FrameGrabber);
            button4.Enabled = false;
            button5.Enabled = false;
            button2.Enabled = false;
            button7.Enabled = false;
            ClearScreen();
            SoundPlayer player = new System.Media.SoundPlayer(Application.StartupPath + @"\Sounds\ok.wav");
            player.Play();
        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            StreamWriter sw = new StreamWriter(Application.StartupPath + "/TrainedFaces/" + name + "/info.txt");
            var fullName = label9.Text.ToString();
            var role = label10.Text.ToString();
            var status = "Вход запрещён";
            var additionalInfo = label12.Text.ToString();
            var information = new List<string>();
            information.Add(fullName);
            information.Add(role);
            information.Add(status);
            information.Add(additionalInfo);
            for (int i = 1; i < information.ToArray().Length + 1; i++)
            {
                sw.WriteLine(information.ToArray()[i - 1]);
            }
            sw.Close();
            grabber.QueryFrame();
            Application.Idle += new EventHandler(FrameGrabber);
            button2.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;
            button7.Enabled = false;
            SoundPlayer player = new System.Media.SoundPlayer(Application.StartupPath + @"\Sounds\prohibited.wav");
            player.Play();
            ClearScreen();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            textBox1.Enabled = true;
            button2.Enabled = true;
            button2.Text = "Добавить";
        }

        private void button2_Click(object sender, System.EventArgs e)
        {
            if (textBox1.Text != "" && string.IsNullOrEmpty(textBox1.Text) == false)
            {
                try
                {
                    if (!isPaused)
                    {
                        //Trained face counter
                        ContTrain = ContTrain + 1;

                        //Get a gray frame from capture device
                        gray = grabber.QueryGrayFrame().Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);

                        //Face Detector
                        MCvAvgComp[][] facesDetected = gray.DetectHaarCascade(
                        face,
                        1.2,
                        10,
                        Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,
                        new Size(20, 20));

                        //Action for each element detected
                        foreach (MCvAvgComp f in facesDetected[0])
                        {
                            TrainedFace = currentFrame.Copy(f.rect).Convert<Gray, byte>();
                            break;
                        }

                        //resize face detected image for force to compare the same size with the 
                        //test image with cubic interpolation type method
                        TrainedFace = result.Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                        trainingImages.Add(TrainedFace);
                        labels.Add(textBox1.Text);

                        //Show face added in gray scale
                        imageBox1.Image = TrainedFace;

                        //Write the number of triained faces in a file text for further load
                        File.WriteAllText(Application.StartupPath + "/TrainedFaces/TrainedLabels.txt", trainingImages.ToArray().Length.ToString() + "%");
                    }
                    var fullName = textBox1.Text.ToString();
                    var role = textBox2.Text.ToString();
                    var status = textBox4.Text.ToString();
                    var additionalInfo = textBox3.Text.ToString();
                    var information = new List<string>();
                    StreamWriter sw;
                    information.Add(fullName);
                    information.Add(role);
                    information.Add(status);
                    information.Add(additionalInfo);
                    //Write the labels of triained faces in a file text for further load
                    if (!isPaused)
                    {
                        for (int i = 1; i < trainingImages.ToArray().Length + 1; i++)
                        {
                            trainingImages.ToArray()[i - 1].Save(Application.StartupPath + "/TrainedFaces/face" + i + ".bmp");
                            File.AppendAllText(Application.StartupPath + "/TrainedFaces/TrainedLabels.txt", labels.ToArray()[i - 1] + "%");
                        }
                    }
                    if (!Directory.Exists(Application.StartupPath + "/TrainedFaces/" + textBox1.Text))
                        Directory.CreateDirectory(Application.StartupPath + "/TrainedFaces/" + textBox1.Text);
                    sw = new StreamWriter(Application.StartupPath + "/TrainedFaces/" + textBox1.Text + "/info.txt");
                    for (int i = 1; i < information.ToArray().Length + 1; i++)
                    {
                        sw.WriteLine(information.ToArray()[i - 1]);
                    }
                    sw.Close();
                    name = textBox1.Text;
                    button3.Enabled = true;
                    MessageBox.Show(textBox1.Text + " добавлен(-а)", "Добавление данных", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    button2.Enabled = false;
                    button7.Enabled = false;
                    UpdateTrainedFaces();
                    button4.Enabled = false;
                    button5.Enabled = false;
                }
                catch
                {
                    MessageBox.Show("Операция недоступна", "Добавление данных", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            else { MessageBox.Show("ФИО не указано.", "Добавление данных", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "" && string.IsNullOrEmpty(textBox1.Text) == false)
            {
                try
                {
                    //Trained face counter
                    ContTrain = ContTrain + 1;
                    //Get a gray frame from capture device
                    gray = grabber.QueryGrayFrame().Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                    //Face Detector
                    MCvAvgComp[][] facesDetected = gray.DetectHaarCascade(
                    face,
                    1.2,
                    10,
                    Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,
                    new Size(20, 20));
                    //Action for each element detected
                    foreach (MCvAvgComp f in facesDetected[0])
                    {
                        TrainedFace = currentFrame.Copy(f.rect).Convert<Gray, byte>();
                        break;
                    }
                    //resize face detected image for force to compare the same size with the 
                    //test image with cubic interpolation type method
                    TrainedFace = result.Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                    trainingImages.Add(TrainedFace);
                    labels.Add(textBox1.Text);
                    //Show face added in gray scale
                    imageBox1.Image = TrainedFace;
                    //Write the number of triained faces in a file text for further load
                    File.WriteAllText(Application.StartupPath + "/TrainedFaces/TrainedLabels.txt", trainingImages.ToArray().Length.ToString() + "%");
                    //Write the labels of triained faces in a file text for further load
                    for (int i = 1; i < trainingImages.ToArray().Length + 1; i++)
                    {
                        trainingImages.ToArray()[i - 1].Save(Application.StartupPath + "/TrainedFaces/face" + i + ".bmp");
                        File.AppendAllText(Application.StartupPath + "/TrainedFaces/TrainedLabels.txt", labels.ToArray()[i - 1] + "%");
                    }
                    name = textBox1.Text;
                    button3.Enabled = true;
                    MessageBox.Show(textBox1.Text + " - добавлены дополнительные фото", "Обновление данных", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    button2.Enabled = false;
                    button7.Enabled = false;
                    UpdateTrainedFaces();
                    button4.Enabled = false;
                    button5.Enabled = false;
                }
                catch
                {
                    MessageBox.Show("Операция недоступна", "Обновление данных", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            } else { MessageBox.Show("ФИО не указано", "Обновление данных", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); }
        }

        void FrameGrabber(object sender, EventArgs e)
        {


            NamePersons.Add("");


            //Get the current frame form capture device
            currentFrame = grabber.QueryFrame().Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);

            //Convert it to Grayscale
            gray = currentFrame.Convert<Gray, Byte>();

            //Face Detector
            MCvAvgComp[][] facesDetected = gray.DetectHaarCascade(
          face,
          1.2,
          10,
          Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,
          new Size(20, 20));

            //Action for each element detected
            foreach (MCvAvgComp f in facesDetected[0])
            {
                t = t + 1;
                result = currentFrame.Copy(f.rect).Convert<Gray, byte>().Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                //draw the face detected in the 0th (gray) channel with blue color
                currentFrame.Draw(f.rect, new Bgr(Color.Red), 2);


                if (trainingImages.ToArray().Length != 0)
                {
                    //TermCriteria for face recognition with numbers of trained images like maxIteration
                    MCvTermCriteria termCrit = new MCvTermCriteria(ContTrain, 0.001);

                    //Eigen face recognizer
                    EigenObjectRecognizer recognizer = new EigenObjectRecognizer(
                       trainingImages.ToArray(),
                       labels.ToArray(),
                       3000,
                       ref termCrit);

                    name = recognizer.Recognize(result);

                    //Draw the label for each face detected and recognized
                    // currentFrame.Draw(name, ref font, new Point(f.rect.X - 2, f.rect.Y - 2), new Bgr(Color.LightGreen));

                }

                NamePersons[t - 1] = name;
                NamePersons.Add("");


                //Set the number of faces detected on the scene


                /*
                //Set the region of interest on the faces

                gray.ROI = f.rect;
                MCvAvgComp[][] eyesDetected = gray.DetectHaarCascade(
                   eye,
                   1.1,
                   10,
                   Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,
                   new Size(20, 20));
                gray.ROI = Rectangle.Empty;

                foreach (MCvAvgComp ey in eyesDetected[0])
                {
                    Rectangle eyeRect = ey.rect;
                    eyeRect.Offset(f.rect.X, f.rect.Y);
                    currentFrame.Draw(eyeRect, new Bgr(Color.Blue), 2);
                }
                 */

            }
            t = 0;

            //Names concatenation of persons recognized
            for (int nnn = 0; nnn < facesDetected[0].Length; nnn++)
            {
                names = names + NamePersons[nnn] + ", ";
            }
            //Show the faces procesed and recognized
            imageBoxFrameGrabber.Image = currentFrame;

            names = "";
            //Clear the list(vector) of names
            NamePersons.Clear();
            if (facesDetected[0].Length != 0)
            {
                button3.Enabled = true;
                Application.Idle -= new EventHandler(FrameGrabber);
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (listBox1.Items.Count == 0)
                button8.Enabled = false;
            else
            {
                RemoveCard(listBox1.SelectedItem.ToString(), FrmPrincipal.loadedLabels);
            }
        }
        private void listBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            if (isPaused)
            {
                if (listBox1.SelectedIndex != -1)
                {
                    button8.Enabled = true;
                    name = listBox1.Items[listBox1.SelectedIndex].ToString();
                    DisplayInfo();
                    textBox1.Text = name;
                    textBox2.Text = label10.Text;
                    textBox4.Text = label11.Text;
                    textBox3.Text = label12.Text;
                    textBox1.Enabled = false;
                    button2.Enabled = true;
                    button2.Text = "Обновить";
                    button4.Enabled = false;
                    button5.Enabled = false;
                }
            }
            if (listBox1.Items.Count==0)
                button8.Enabled = false;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (label9.Text!="" && string.IsNullOrEmpty(label9.Text) ==false && label9.Text != "Этого человека нет в базе данных")
            {
                openFileDialog2.Filter = "jpg files (*.jpg)|*.jpg";
                openFileDialog2.ShowDialog();
            }
            
        }
        private void openFileDialog2_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            pictureBox1.Image = null;
            pictureBox1.ImageLocation = null;
            if (pictureBoxImg != null)
                pictureBoxImg.Dispose();
            if (File.Exists(Application.StartupPath + @"\TrainedFaces\" + label9.Text + @"\" + label9.Text + ".jpg"))
                File.Delete(Application.StartupPath + @"\TrainedFaces\" + label9.Text + @"\" + label9.Text + ".jpg");
            File.Copy(openFileDialog2.FileName, Application.StartupPath + @"\TrainedFaces\" + label9.Text + @"\" + label9.Text + ".jpg");
            DisplayInfo();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            Application.Idle -= new EventHandler(FrameGrabber);
            button9.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;
            button10.Enabled = true;
            isPaused = true;
        }

        private void button10_Click(object sender, EventArgs e)
        {
            grabber.QueryFrame();
            Application.Idle += new EventHandler(FrameGrabber);
            button9.Enabled = true;
            button10.Enabled = false;
            isPaused = false;
            button8.Enabled = false;
        }

        public void DisplayInfo()
        {
            if (File.Exists(Application.StartupPath + "/TrainedFaces/" + name + "/info.txt"))
            {
                StreamReader personalInfo = new StreamReader(Application.StartupPath + "/TrainedFaces/" + name + "/info.txt");
                var fullName = personalInfo.ReadLine();
                var role = personalInfo.ReadLine();
                var status = personalInfo.ReadLine();
                var additional = personalInfo.ReadLine();
                label9.Text = fullName;
                label10.Text = role;
                label11.Text = status;
                label12.Text = additional;
                if (File.Exists(Application.StartupPath + "/TrainedFaces/" + name + "/" + name + ".jpg"))
                {
                    pictureBoxImg = Image.FromFile(Application.StartupPath + "/TrainedFaces/" + name + "/" + name + ".jpg");
                    pictureBox1.Image = pictureBoxImg;
                   
                } else
                {
                    pictureBox1.Image = null;
                    pictureBox1.ImageLocation = null;
                    if (pictureBoxImg != null)
                        pictureBoxImg.Dispose();
                }
                button4.Enabled = true;
                button5.Enabled = true;
                personalInfo.Close();
            } else
            {
                label9.Text = "Этого человека нет в базе данных";
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DisplayInfo();
            if (label9.Text != "Этого человека нет в базе данных")
            {
                textBox1.Text = name;
                textBox2.Text = label10.Text;
                textBox4.Text = label11.Text;
                textBox3.Text = label12.Text;
                textBox1.Enabled = false;
                button2.Text = "Обновить";
                button6.Visible = true;
                button6.Enabled = true;
                SoundPlayer player = new System.Media.SoundPlayer(Application.StartupPath + @"\Sounds\found.wav");
                player.Play();
                button2.Enabled = false;
            }
            else
            {
                SoundPlayer player = new System.Media.SoundPlayer(Application.StartupPath + @"\Sounds\notfound.wav");
                player.Play();
                textBox1.Enabled = true;
                button2.Text = "Добавить";
                button2.Enabled = true;
            }
            button3.Enabled = false;
            button7.Enabled = true;
        }
        private void ClearScreen()
        {
            textBox1.Text = "";
            textBox2.Text = "";
            textBox4.Text = "";
            textBox3.Text = "";
            textBox1.Enabled = true;
            button6.Enabled = false;
            button6.Visible = false;
            label9.Text = "";
            label10.Text = "";
            label11.Text = "";
            label12.Text = "";
            pictureBox1.Image = null;
            pictureBox1.ImageLocation = null;
            if (pictureBoxImg != null)
                pictureBoxImg.Dispose();
        }
        private void RemoveCard(string name, string[] Labels)
        {
            ClearScreen();
            if (pictureBoxImg != null)
                pictureBoxImg.Dispose();
            try { Directory.Delete(Application.StartupPath + "/TrainedFaces/" + name, true); }
            catch { /*MessageBox.Show("Ошибка удаления директории. Попробуйте позже.", "Обновление данных", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);*/ }
            List<string> newTxt = new List<string>();
            string result="";
            List<int> deletedFaces = new List<int>();
            Labels = File.ReadAllText(Application.StartupPath + "/TrainedFaces/TrainedLabels.txt").Split('%');
            for (int i = 1; i <= Int32.Parse(Labels[0]); i++)
            {
                if (Labels[i] == name)
                {
                    File.Delete(Application.StartupPath + "/TrainedFaces/" + "face" + i + ".bmp");
                    deletedFaces.Add(i);
                }
                else if (Labels[i] != name && !string.IsNullOrEmpty(Labels[i])&& Labels[i]!=" ")
                    newTxt.Add(Labels[i]);
            }
            result += newTxt.Count +"%";
            for (int i = 0; i < newTxt.Count; i++)
            {
                result += newTxt[i] + "%";
            }
            StreamWriter sw = new StreamWriter(Application.StartupPath + "/TrainedFaces/TrainedLabels.txt");
            sw.WriteLine(result);
            sw.Close();
            deletedFaces.Reverse();
            foreach (var e in deletedFaces)
            { 
                for (int j = e; j <= Labels.Length; j++)
                {
                    var h = j + 1;
                    if (File.Exists(Application.StartupPath + "/TrainedFaces/" + "face" + h + ".bmp"))
                    {
                        File.Move(Application.StartupPath + @"\TrainedFaces\" + "face" + h + ".bmp", Application.StartupPath + @"\TrainedFaces\" + "face" + j + ".bmp");
                    }
                }
            }
            UpdateTrainedFaces();
        }
    }

}