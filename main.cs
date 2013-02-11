namespace FreeStyle{
    using System;
    using System.Windows.Forms;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.IO;
    public class RGSS{
      [DllImport("Kernel32")]
      public static extern void ExitProcess(int code);

      [DllImport("RGSS301", CharSet=CharSet.Unicode, EntryPoint = "RGSSGameMain")]
      public static extern void GameMain(IntPtr a, string b, string c);

      [DllImport("RGSS301", EntryPoint = "RGSSInitialize3")]
      public static extern void Initialize(int a);

      [DllImport("RGSS301", EntryPoint = "RGSSFinalize")]
      public static extern void Uninitialize();


      [DllImport("RGSS301", CharSet=CharSet.Unicode, EntryPoint = "RGSSSetupRTP")]
      public static extern int SetupRTP(string ini, StringBuilder error, int errlen);

      [DllImport("RGSS301", CharSet=CharSet.Ansi, EntryPoint = "RGSSEval")]
      public static extern int Eval(string text);

   };
   public class FreeStyleForm : Form {
    Form f;
    RichTextBox rtb;
    Panel pn1, pn2;
    int count;

    public FreeStyleForm(){
      init_vars();
      make_textarea();
      make_toolbar();
      setup_windows();
      setup_va_script();
      setup_va();
    }

    void init_vars(){
      count = 0;
    }
   
    void make_textarea(){
     pn1 = new Panel();
     pn1.Dock = DockStyle.Fill;

     rtb = new RichTextBox();
     rtb.Dock = DockStyle.Fill;
     rtb.Font = new Font("VL Gothic", 14, FontStyle.Bold);
     
     pn1.Controls.Add(rtb);

     Controls.Add(pn1);
    }

    void make_toolbar(){
     pn2 = new Panel();
     pn2.Dock  = DockStyle.Top;
     pn2.Height = 20;
     var rb = new Regex(@"\.rb$", RegexOptions.IgnoreCase);
     foreach(var x in Directory.GetFiles(System.Environment.CurrentDirectory + "/plugins")){
        if(rb.IsMatch(x)){
           add_tool( (bt)=>{
              bt.Text = System.IO.Path.GetFileName(x);
              bt.Click += (o, e)=>{
                using(var file = new StreamReader(x)){
                  rtb.Text+=file.ReadToEnd();
                };
              };
           });
        }
     }
     add_tool( (bt) => {
              bt.Text = "Exit";
              bt.Click += (o, e)=>{
                  exit();
              };
        }
     );

     add_tool( (bt) => {
              bt.Text = "Run";
              bt.Click += (o, e)=>{
                  run();
              };
      });

     Controls.Add(pn2);
    }

    void setup_windows(){


      Closing += (o, e) => exit();
      Text = "FreeStyle v1.00";
      Show();

      f = new Form();
      f.Closing += (o, e) => exit();
      f.Show();

    }

    void setup_va_script(){
      using(var file = new System.IO.StreamWriter("main.rb")){
          file.WriteLine(@"
            $mtime = File.read('main.rb')[/.+\n/]
            Win32API.new('Kernel32', 'AllocConsole', '', 'i').call
            STDIN.reopen('CONIN$')
            STDOUT.reopen('CONOUT$')
            STDERR.reopen('CONOUT$')
            Thread.new do loop do Graphics.update end end
             loop do
              text = File.read('main.rb')
              r = text[/.+\n/]
              if $mtime != r
               $mtime = r
               begin
                  eval(text, binding, 'main', 1)
               rescue SystemExit
                  break
               rescue Object => ex
                  puts ex.to_s
                  puts ex.backtrace
               end
              end
             end

          ");
      }
    }

    void setup_va(){
     new Thread( ()=>{
       RGSS.Initialize(0); 
       StringBuilder sb = new StringBuilder(1024);
       if(RGSS.SetupRTP(System.Environment.CurrentDirectory+"/Game.ini", sb, 1024) == 0){
           Console.WriteLine("Can't find RTP {0}", sb.ToString());
       }
       
       RGSS.Eval("Marshal.dump([[0,0,Zlib::Deflate.deflate(File.read('main.rb'))]], open('1.a','wb')).close");
       RGSS.GameMain(f.Handle, "1.a", "\0\0\0\0");
     }).Start();
    }

    void exit(){
      using(var file = new System.IO.StreamWriter("main.rb")){
          file.WriteLine("exit");
       }
    }

    void run(){
      using(var file = new System.IO.StreamWriter("main.rb")){
          ++count;
          file.WriteLine("#"+count.ToString());
          file.WriteLine(rtb.Text);
       }
    }

    void add_tool(Action<Button> fn){
       Button bt = new Button();
       bt.Dock = DockStyle.Left;
       fn(bt);
       pn2.Controls.Add(bt);
    }
  }


  public class App{
     public static void Main(String []args){       
       Application.EnableVisualStyles();
       Application.Run(new FreeStyleForm());
     }
  }
}

