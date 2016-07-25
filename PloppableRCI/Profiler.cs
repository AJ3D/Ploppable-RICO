using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualBasic;
using System.Diagnostics;

#if DEBUG
using PostSharp.Aspects;
#endif

namespace PloppableRICO
{
#if DEBUG
    [Serializable]
    public class MethodTraceAspect : OnMethodBoundaryAspect
    {
        public override void OnEntry( MethodExecutionArgs args )
        {
            Console.WriteLine( args.Method.Name + " started" );
        }

        public override void OnExit( MethodExecutionArgs args )
        {
            Console.WriteLine( args.Method.Name + " finished" );
        }
    }

    [Serializable]
    [ProfilerAspect( AttributeExclude = true )]
    public class ProfilerAspect : PostSharp.Aspects.OnMethodBoundaryAspect
    {
        public override void OnEntry( MethodExecutionArgs args )
        {
            string output = string.Format("Executing {0}\r\n", args.Method.Name);
            Profiler.Write( output );
            Profiler.indentation++;

            args.MethodExecutionTag = Stopwatch.StartNew();
        }

        public override void OnExit( MethodExecutionArgs args )
        {
            Stopwatch sw = (Stopwatch)args.MethodExecutionTag;
            sw.Stop();

            string output = string.Format("{0} Executed in {1} milliseconds\r\n",
                             args.Method.Name, sw.ElapsedMilliseconds);

            Profiler.indentation--;
            Profiler.Write( output );

        }
    }

    public class Profiler
    {
        public static Profiler Instance;
        public static String OutputFileName;
        public static int indentation = 0;
        private DateTime StartTime;
        private DateTime EndTime;
        private Dictionary<string, DateTime> Timers = new Dictionary<string, DateTime>();


        public static void Write( string s )
        {
            if ( OutputFileName == null )
                OutputFileName = Path.Combine(
                    Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ),
                    @"Colossal Order\Cities_Skylines\PloppableRico.trace"
                );

            if ( Instance == null )
                Instance = new Profiler( OutputFileName );

            Instance.WriteString( new string( ' ', indentation ) + s );
        }

        public static void Message( string msg )
        {
            msg += "\r\n";
            Profiler.Write( msg );
        }

        public static void Info( string message )
        {
            Profiler.Write( " [INFO] " + message + "\r\n" );
        }

        public static void Debug( string variable, string value )
        {
            Profiler.Write( String.Format( "{0} : {1}\r\n", variable, value ) ); ;
        }

        public void WriteString( string s )
        {
            var b = System.Text.Encoding.Unicode.GetBytes( String.Format("{0:MM-dd-yyyy H:mm:ss} {1}", DateTime.Now, s ) );
            _outputStream.Write( b, 0, b.Length );
            _outputStream.Flush();
        }

        string _outputFileName;
        Stream _outputStream;

        public Profiler( string outputFileName )
        {
            StartTime = DateTime.Now;
            _outputFileName = outputFileName;
            _outputStream = new FileStream( _outputFileName, FileMode.Append, FileAccess.Write );
            this.WriteString( "Profiler started\r\n" );
        }

        public Profiler( Stream outputStream )
        {
            StartTime = DateTime.Now;
            _outputStream = outputStream;
            this.WriteString( "Profiler started\r\n" );
        }

        ~Profiler()
        {
            if ( _outputStream != null )
            {
                EndTime = DateTime.Now;
                var delta = EndTime - StartTime;

                try
                {
                    var outputStream = new FileStream( OutputFileName, FileMode.Append, FileAccess.Write );
                    var s = String.Format( "{0:MM-dd-yyyy H:mm:ss} Profiler ended after {1}:{2},{3}\r\n", DateTime.Now, delta.Minutes, delta.Seconds, delta.Milliseconds );
                    var b = System.Text.Encoding.Unicode.GetBytes( new string( ' ', indentation ) + s  );
                    outputStream.Write( b, 0, b.Length );
                    outputStream.Flush();
                    outputStream.Close();
                    _outputStream.Close();
                }
                catch
                {

                }
            }
        }
    }
#else
    public class Profiler
    {
        public static void Write( string s )
        {
            Console.Write(s);
        }

        public static void Message( string msg )
        {
            msg += "\r\n";
            Profiler.Write( msg );
        }

        public static void Info( string message )
        {
            Profiler.Write( " [INFO] " + message + "\r\n" );
        }

        public static void Debug( string variable, string value )
        {
            Profiler.Write( String.Format( "{0} : {1}\r\n", variable, value ) ); ;
        }
    }
#endif
}
