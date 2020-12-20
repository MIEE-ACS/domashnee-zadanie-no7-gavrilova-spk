using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.IO;

namespace Snake
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Поле на котором живет змея
        Entity field;
        // голова змеи
        Head head;
        // вся змея
        List<PositionedEntity> snake;
        // яблоко
        Apple apple;
        //количество очков
        List<Box> boxes;
        int score;
        //таймер по которому 
        DispatcherTimer moveTimer;

        //конструктор формы, выполняется при запуске программы
        public MainWindow()
        {
            InitializeComponent();
            
            snake = new List<PositionedEntity>();
            boxes = new List<Box>();
            //создаем поле 300х300 пикселей
            field = new Entity(600, 600, "pack://application:,,,/Resources/snake.png");

            //создаем таймер срабатывающий раз в 300 мс
            moveTimer = new DispatcherTimer();
            moveTimer.Interval = new TimeSpan(0, 0, 0, 0, 300);
            moveTimer.Tick += new EventHandler(moveTimer_Tick);            
        }

       

        //метод перерисовывающий экран
        private void UpdateField()
        {
            //обновляем положение элементов змеи
            foreach (var p in snake)
            {
                Canvas.SetTop(p.image, p.y);
                Canvas.SetLeft(p.image, p.x);
            }

            //обновляем положение яблока
            Canvas.SetTop(apple.image, apple.y);
            Canvas.SetLeft(apple.image, apple.x);

            //обновляем положение препятствий
            foreach (var b in boxes)
            {
                Canvas.SetTop(b.image, b.y);
                Canvas.SetLeft(b.image, b.x);
            }

            //обновляем количество очков
            lblScore.Content = String.Format("{0}000", score);

        }

        //обработчик тика таймера. Все движение происходит здесь
        void moveTimer_Tick(object sender, EventArgs e)
        {
            //в обратном порядке двигаем все элементы змеи
            foreach (var p in Enumerable.Reverse(snake))
            {
                p.move();
            }

            //проверяем, что голова змеи не врезалась в тело
            foreach (var p in snake.Where(x => x != head))
            {
                //если координаты головы и какой либо из частей тела совпадают
                if (p.x == head.x && p.y == head.y)
                {
                    //мы проиграли
                    moveTimer.Stop();
                    tbGameOver.Visibility = Visibility.Visible;
                    txt_error.Text = "self";
                    return;
                }
            }

            //проверяем, что голова змеи не вышла за пределы поля
            if (head.x < 40 || head.x >= 540 || head.y < 40 || head.y >= 540)
            {
                //мы проиграли
                moveTimer.Stop();
                tbGameOver.Visibility = Visibility.Visible;
                txt_error.Text = "wall";
                return;
            }

            //проверяем, что голова змеи врезалась в яблоко!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            if (head.x == apple.x && head.y == apple.y)
            {
                //увеличиваем счет
                score++;
                //двигаем яблоко на новое место
                apple.move();
                // добавляем новый сегмент к змее
                var part = new BodyPart(snake.Last());
                canvas1.Children.Add(part.image);
                snake.Add(part);


                Random rand = new Random();
                int trouble = rand.Next(100);  //вероятность выпадания препятствия ~2/3
                if( trouble < 60)
                {
                        var tr = new Box(snake, apple);
                        canvas1.Children.Add(tr.image);
                        boxes.Add(tr);
                }
            }

            //проверяем, что голова змеи врезалась в препятствие !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            foreach (var b in boxes)
            {
                //если координаты головы и какой либо из box совпадают
                if (b.x == head.x && b.y == head.y)
                {
                    //мы проиграли
                    moveTimer.Stop();
                    tbGameOver.Visibility = Visibility.Visible;
                    txt_error.Text = "box?";
                    return;
                }
            }

            if (score % 25 == 0)
            {
                canvas1.Children.Clear();
                boxes.Clear();
                canvas1.Children.Add(field.image);
                // добавлем яблоко
                canvas1.Children.Add(apple.image);
                // добавлем змею
                foreach (var p in snake)
                {
                    canvas1.Children.Add(p.image);
                }
            }

            //перерисовываем экран
            UpdateField();
        }



        // Обработчик нажатия на кнопку клавиатуры
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                        head.direction = Head.Direction.UP;
                    break;
                case Key.Down:
                        head.direction = Head.Direction.DOWN;
                    break;
                case Key.Left:
                        head.direction = Head.Direction.LEFT;
                    break;
                case Key.Right:
                        head.direction = Head.Direction.RIGHT;
                    break;
            }
        }

        // Обработчик нажатия кнопки "Start"
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            // обнуляем счет
            score = 0;
            // обнуляем змею
            snake.Clear();
            // обнуляем box
            boxes.Clear();
            // очищаем канвас
            canvas1.Children.Clear();
            // скрываем надпись "Game Over"
            tbGameOver.Visibility = Visibility.Hidden;
            
            // добавляем поле на канвас
            canvas1.Children.Add(field.image);
            // создаем новое яблоко и добавлем его
            apple = new Apple(snake, boxes);
            canvas1.Children.Add(apple.image);
            // создаем голову
            head = new Head();
            snake.Add(head);
            canvas1.Children.Add(head.image);

            //запускаем таймер
            moveTimer.Start();
            UpdateField();

        }
        
        public class Entity
        {
            protected int m_width;
            protected int m_height;
            
            Image m_image;
            public Entity(int w, int h, string image)
            {
                m_width = w;
                m_height = h;
                m_image = new Image();
                m_image.Source = (new ImageSourceConverter()).ConvertFromString(image) as ImageSource;
                m_image.Width = w;
                m_image.Height = h;

            }

            public Image image
            {
                get
                {
                    return m_image;
                }
            }
        }

        public class PositionedEntity : Entity
        {
            protected int m_x;
            protected int m_y;
            public PositionedEntity(int x, int y, int w, int h, string image)
                : base(w, h, image)
            {
                m_x = x;
                m_y = y;
            }

            public virtual void move() { }

            public int x
            {
                get
                {
                    return m_x;
                }
                set
                {
                    m_x = value;
                }
            }

            public int y
            {
                get
                {
                    return m_y;
                }
                set
                {
                    m_y = value;
                }
            }
        }

        public class Apple : PositionedEntity
        {
            List<PositionedEntity> m_snake;
            List<Box> n_boxes;
            public Apple(List<PositionedEntity> s, List<Box> b)
                : base(0, 0, 40, 40, "pack://application:,,,/Resources/fruit.png")
            {
                m_snake = s;
                n_boxes = b;
                move();
            }

            public override void move()
            {
                Random rand = new Random();
                do
                {
                    x = rand.Next(13) * 40 + 40;
                    y = rand.Next(13) * 40 + 40;
                    bool overlap = false;
                    foreach (var p in m_snake)
                    {
                        if (p.x == x && p.y == y)
                        {
                            overlap = true;
                            break;
                        }
                    }
                    foreach (var b in n_boxes)
                    {
                        if (x >= b.x - 40 && x <= b.x + 40)
                        {
                            if (y >= b.y - 40 && y <= b.y + 40)
                            { 
                                overlap = true;
                            break;
                            }
                        }
                    }
                    if (!overlap)
                        break;
                } while (true);

            }
        }

        public class Head : PositionedEntity
        {
            public enum Direction
            {
                RIGHT, DOWN, LEFT, UP, NONE
            };

            Direction m_direction;

            public Direction direction {
                set
                {
                    m_direction = value;
                    RotateTransform rotateTransform = new RotateTransform(90 * (int)value);
                    image.RenderTransform = rotateTransform;
                }
            }

            public Head()
                : base(280, 280, 40, 40, "pack://application:,,,/Resources/head.png")
            {
                image.RenderTransformOrigin = new Point(0.5, 0.5);
                m_direction = Direction.NONE;
            }

            public override void move()
            {
                switch (m_direction)
                {
                    case Direction.DOWN:
                        y += 40;
                        break;
                    case Direction.UP:
                        y -= 40;
                        break;
                    case Direction.LEFT:
                        x -= 40;
                        break;
                    case Direction.RIGHT:
                        x += 40;
                        break;
                }
            }
        }

        public class BodyPart : PositionedEntity
        {
            PositionedEntity m_next;
            public BodyPart(PositionedEntity next)
                : base(next.x, next.y, 40, 40, "pack://application:,,,/Resources/body.png")
            {
                m_next = next;
            }

            public override void move()
            {
                x = m_next.x;
                y = m_next.y;
            }
        }

        public class Box : PositionedEntity
        {
            List<PositionedEntity> m_snake;
            public Box(List<PositionedEntity> s, Apple a)
                : base(0, 0, 40, 40, "pack://application:,,,/Resources/Box.png")
            {
                m_snake = s;
                troublemaker(a);
            }

            public void troublemaker(Apple a)
            {
                Random rnd = new Random();
                do
                {
                    x = rnd.Next(13) * 40 + 40;
                    y = rnd.Next(13) * 40 + 40;
                    bool overlap = false;
                    if ( a.x == x && a.y == y)
                        overlap = true;

                    foreach (var p in m_snake)
                    {
                        if (x >= p.x - 40 && x <= p.x + 40 )
                        {
                            if (y >= p.y - 40 && y <= p.y + 40)
                            {
                                overlap = true;
                                break;
                            }
                        }
                    }
                    if (!overlap)
                        break;
                } while (true);

            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            moveTimer.Stop();
            btm_start.Visibility = Visibility.Visible;
            btm_pause.Visibility = Visibility.Hidden;

        }

        private void Btm_start_Click(object sender, RoutedEventArgs e)
        {
            btm_pause.Visibility = Visibility.Visible;
            btm_start.Visibility = Visibility.Hidden;
            moveTimer.Start();
            UpdateField();
        }
        
    }
}
