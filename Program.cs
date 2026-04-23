// Javier Hoyos Giunta
// Hector Prous Arroyo
using System.ComponentModel.Design;
using System.Drawing;
using System.Reflection.Metadata;
using Coordinates;
using SetArray;
using System.IO;

namespace Buscamingas
{
    internal class Program
    {
        public class Tablero
        {
            // Casillas de juego
            private struct Casilla
            {
                public char estado; // ’o’ sin descubrir, ’1’-’8’ número de minas adyacentes, ’*’ marcado como mina
                                    // ’x’ mina descubierta/explotada, ’·’ descubierto sin minas adyacentes
                public bool mina; // true: hay mina; false eoc
            }
            private int fils, cols; // numero de filas y columnas del tablero
            private Casilla[,] casilla; // matriz de casillas del tablero
            private Coor cursor; // posición del cursor
            private int nMinas, nMarcadas; // número de minas y número de casillas marcadas
            private bool primerClick; // para garantizar que el primer click no sea una mina
            private bool debug; // para depuración, el Renderizado muestra las minas en el tablero
            static Random rnd = new Random(); // generador de aleatorios para colocar las minas

            public int Fils()
            {
                return fils;
            }

            public int Cols()
            {
                return cols;
            }

            public Tablero(int fils, int cols, int numMinas)
            {
                this.fils = fils;
                this.cols = cols;
                this.nMinas = numMinas;
                this.nMarcadas = 0;
                this.cursor = new Coor(0, 0);
                this.primerClick = true;
                this.debug = false;

                casilla = new Casilla[fils, cols];
                for (int i = 0; i < fils; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        casilla[i, j].estado = 'o';
                        casilla[i, j].mina = false;
                    }
                }
                ponMinas1(numMinas);
            }

            private void ponMinas1(int nMinas)
            {
                // ponemos las minas en las primeras nMinas casillas de arriba abajo y de izq a drch
                int count = 0;
                for (int i = 0; i < fils && count < nMinas; i++)
                {
                    for (int j = 0; j < cols && count < nMinas; j++)
                    {
                        casilla[i, j].mina = true;
                        count++;
                    }
                }
                //hacemos el shuffle solo para las primeras nMinas de arriba abajo y de izq a drch
                count = 0;
                for (int i = 0; i < nMinas; i++)
                {
                    int orMinaFil = i / cols;
                    int orMinaCol = i % cols;

                    // elegimos una casilla aleatoria desde i (mina actual) hasta el final del tablero
                    int r = rnd.Next(i, fils * cols);
                    int rMinaFil = r / cols;
                    int rMinaCol = r % cols;

                    while (casilla[rMinaFil, rMinaCol].estado != 'o')
                    {
                        r = rnd.Next(i, fils * cols);
                        rMinaFil = r / cols;
                        rMinaCol = r % cols;
                    }

                    // intercambiamos mina original por la mina randomizada y viceversa
                    bool registerOrMina = casilla[orMinaFil, orMinaCol].mina;
                    casilla[orMinaFil, orMinaCol].mina = casilla[rMinaFil, rMinaCol].mina;
                    casilla[rMinaFil, rMinaCol].mina = registerOrMina;

                }
            }

            public Tablero(int fils, int cols, (int, int)[] posMinas)
            {
                this.fils = fils;
                this.cols = cols;
                this.cursor = new Coor(0, 0);
                casilla = new Casilla[fils, cols];
                for (int i = 0; i < fils; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        casilla[i, j].estado = 'o';
                        casilla[i, j].mina = false;
                    }
                }

                for (int j = 0; j < posMinas.Length; j++)
                {
                    casilla[posMinas[j].Item1, posMinas[j].Item2].mina = true;
                }
            }

            public void activateDEBUG()
            {
                debug = !debug;
            }

            public void Render(bool bomba)
            {
                Console.Clear();
                Console.WriteLine($" Minas: {nMinas - nMarcadas}");
                Console.WriteLine();

                for (int i = 0; i < fils; i++)
                {
                    Console.Write(" ");
                    for (int j = 0; j < cols; j++)
                    {
                        if (i == cursor.X && j == cursor.Y)
                        {
                            Console.BackgroundColor = ConsoleColor.Gray;
                            Console.ForegroundColor = ConsoleColor.Black;
                        }

                        Console.ForegroundColor = ProcesaPigmentos(casilla[i, j].estado);
                        if (debug || bomba)
                        {
                            if (casilla[i, j].mina && casilla[i, j].estado != 'x')
                            {
                                Console.BackgroundColor = ConsoleColor.Red;
                                Console.Write('*');
                            }

                            else
                                Console.Write(casilla[i, j].estado);
                        }
                        else
                        {
                            Console.Write(casilla[i, j].estado);
                        }

                        Console.ResetColor();
                        Console.Write(' ');
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();
            }


            public void MueveCursor(Coor dir)
            {
                cursor.X = Math.Clamp(cursor.X + dir.X, 0, fils - 1);
                cursor.Y = Math.Clamp(cursor.Y + dir.Y, 0, cols - 1);
            }

            public void MarcaMina()
            {
                if (nMarcadas < nMinas)
                {
                    if (casilla[cursor.X, cursor.Y].estado == 'o')
                    {
                        casilla[cursor.X, cursor.Y].estado = 'x';
                        nMarcadas++;
                    }
                    else if (casilla[cursor.X, cursor.Y].estado == 'x')
                    {
                        casilla[cursor.X, cursor.Y].estado = 'o';
                        nMarcadas--;
                    }
                }
            }

            public bool ClickCasilla()
            {
                if (casilla[cursor.X, cursor.Y].estado == 'o')
                {
                    int minas = MinasAlrededor(cursor.X, cursor.Y);
                    if (casilla[cursor.X, cursor.Y].mina)
                    {
                        if (primerClick)
                        {
                            casilla[cursor.X, cursor.Y].mina = false;
                            if (minas == 0)
                            {
                                casilla[cursor.X, cursor.Y].estado = '.';
                                DescubreAdyacentes();
                            }
                            else
                            {
                                char c = (char)(minas + '0');
                                casilla[cursor.X, cursor.Y].estado = c;
                            }
                            ponMinas1(1);
                        }
                        else casilla[cursor.X, cursor.Y].estado = '*';
                    }
                    else
                    {
                        if (minas == 0)
                        {
                            casilla[cursor.X, cursor.Y].estado = '.';
                            DescubreAdyacentes();
                        }

                        else
                        {
                            char c = (char)(minas + '0');
                            casilla[cursor.X, cursor.Y].estado = c;
                        }
                    }
                }

                if (primerClick)
                {
                    primerClick = false;
                }

                return casilla[cursor.X, cursor.Y].mina;
            }

            private int MinasAlrededor(int x, int y)
            {

                int numMinas = 0;

                int xMin = Math.Clamp(x - 1, 0, fils - 1);
                int xMax = Math.Clamp(x + 1, 0, fils - 1);
                int yMin = Math.Clamp(y - 1, 0, cols - 1);
                int yMax = Math.Clamp(y + 1, 0, cols - 1);

                for (int i = xMin; i <= xMax; i++)
                {
                    for (int j = yMin; j <= yMax; j++)
                    {
                        if (casilla[i, j].mina)
                            numMinas++;
                    }
                }

                if (casilla[x, y].mina)
                {
                    numMinas--;
                }

                return numMinas;
            }

            private void DescubreAdyacentes()
            {
                SetCoor pendientes = new SetCoor(casilla.Length);
                SetCoor visitadas = new SetCoor(casilla.Length);

                pendientes.Add(cursor);

                while (pendientes.GetOc() > 0)
                {
                    Coor actual = pendientes.PopElem();
                    visitadas.Add(actual);


                    if (MinasAlrededor(actual.X, actual.Y) == 0)
                    {
                        int xMin = Math.Clamp(actual.X - 1, 0, fils - 1);
                        int xMax = Math.Clamp(actual.X + 1, 0, fils - 1);
                        int yMin = Math.Clamp(actual.Y - 1, 0, cols - 1);
                        int yMax = Math.Clamp(actual.Y + 1, 0, cols - 1);

                        for (int i = xMin; i <= xMax; i++)
                        {
                            for (int j = yMin; j <= yMax; j++)
                            {
                                Coor ady = new Coor(i, j);

                                if (!visitadas.Belongs(ady))
                                {
                                    pendientes.Add(ady);
                                    if (casilla[i, j].estado == 'x')
                                    {
                                        nMarcadas--;
                                    }
                                    casilla[i, j].estado = (char)(MinasAlrededor(i, j) + '0');

                                }
                            }
                        }
                    }
                }
            }

            public bool Terminado()
            {
                int i = 0, j = 0;
                bool ok = true;

                while (i < fils && ok)
                {
                    while (j < cols)
                    {
                        if (!casilla[i, j].mina && (casilla[i, j].estado == 'o' || casilla[i, j].estado == 'x'))
                        {
                            ok = false;
                        }
                        j++;
                    }
                    i++;
                    j = 0;
                }
                return ok;
            }

            public string CodificaTablero()
            {
                int[] listaMinas = new int[nMinas];
                string lTab = "";
                int pos = 0, k = 0;

                // añade estado de todas las casillas
                for (int i = 1; i < casilla.GetLength(0); i++)
                {
                    for (int j = 1; j < casilla.GetLength(1); j++)
                    {
                        lTab += (char)casilla[i, j].estado;
                        if (casilla[i, j].mina)
                        {
                            lTab += '#'; // para marcar que esta casilla es mina
                        }

                    }
                    lTab += '&'; // para marcar salto de línea
                }

                lTab += '@'; // para marcar cambio de categoría

                // añade cursor actual
                lTab += cursor.X;
                lTab += +cursor.Y;

                return lTab;
            }

            private ConsoleColor ProcesaPigmentos(char c)
            {
                ConsoleColor color;
                ConsoleColor[] listaColores = (ConsoleColor[])ConsoleColor.GetValues(typeof(ConsoleColor));

                if (c == 'o')
                {
                    color = ConsoleColor.Gray;
                }
                else if (c == 'x')
                {
                    color = ConsoleColor.Red;
                }
                else if (c == '.')
                {
                    color = ConsoleColor.Black;
                }
                else if (c == '*')
                {
                    color = ConsoleColor.White;
                }
                else
                {
                    color = listaColores[c - 48];
                }

                return color;
            }

            public void GuardaPartida(string nombreArchivo)
            {
                StreamWriter sw = new StreamWriter(nombreArchivo);
                try
                {
                    sw.WriteLine(CodificaTablero());
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error: {e.Message}");
                }
                finally
                {
                    if (sw != null) sw.Close();
                }
            }
        }

        static bool ProcesaInput(Tablero t, char c)
        {
            Coor up, down, left, right;
            bool bomba = false;

            left = new Coor(0, -1);
            right = new Coor(0, 1);
            up = new Coor(-1, 0);
            down = new Coor(1, 0);

            switch (c)
            {
                case 'l':
                    t.MueveCursor(left);
                    break;

                case 'u':
                    t.MueveCursor(up);
                    break;

                case 'r':
                    t.MueveCursor(right);
                    break;

                case 'd':
                    t.MueveCursor(down);
                    break;

                case 'x':
                    t.MarcaMina();
                    break;

                case 'c':
                    bomba = t.ClickCasilla();

                    break;

                case 'v':
                    t.activateDEBUG();
                    break;
            }
            return bomba;
        }

        public static char LeeInput()
        {
            char d = ' ';
            string tecla = Console.ReadKey(true).Key.ToString();
            switch (tecla)
            {
                case "LeftArrow": d = 'l'; break; // izda
                case "UpArrow": d = 'u'; break; // arriba
                case "RightArrow": d = 'r'; break; // dcha
                case "DownArrow": d = 'd'; break; // abajo
                case "Spacebar": d = 'c'; break; // click para destapar
                case "Enter": d = 'x'; break; // marca/desmarca mina
                case "Escape": d = 'q'; break; // abandonar partida
                case "D": d = 'v'; break; // activar y desactivar debug
            }
            while (Console.KeyAvailable) Console.ReadKey().Key.ToString();
            return d;
        }

        static void Juego(Tablero t, char input)
        {
            bool bomba = false;
            t.Render(false);

            while (input != 'q' && !bomba && !t.Terminado())
            {
                input = LeeInput();
                bomba = ProcesaInput(t, input);
                t.Render(bomba);
            }

            if (bomba) Console.WriteLine("BOOM!");
            else if (t.Terminado()) Console.WriteLine("¡Victoria!");
            else
            {
                Console.Write("Has abandonado... ¿Deseas guardar la partida? (s/n): ");
                string st = Console.ReadLine();

                if (st == "s")
                {
                    Console.Write("Introduce el nombre con el que quieres que se guarde: ");
                    string nombre = Console.ReadLine();
                    t.GuardaPartida(nombre);
                }

            }
        }

        public static Tablero RecuperaPartida(string nombreArchivo)
        {
            Tablero t = null;
            int i = 0, j = 0;
            bool salto = false, mina = false, cursor = false;
            char c = ' ';

            StreamReader archivo = null;
            try
            {
                archivo = new StreamReader(nombreArchivo);
                while (!cursor)
                {
                    c = (char)archivo.Read();
                    while (!salto)
                    {
                        if (c == '@')
                        {
                            cursor = true;
                        }
                        else
                        {
                            if (c == '&')
                            {
                                salto = true;
                            }
                            else
                            {
                                if (c == '#')
                                {
                                       
                                }
                                else
                                {
                                    // aquí lee los estados de las casillas del archivo

                                    // temporalmente para que no se quede infinitamente:
                                    cursor = true;
                                    salto = true;
                                }
                            }
                        }
                        j++;
                    }
                    i++;
                }

            }
            catch(Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }
            finally 
            { 
                if (archivo != null) archivo.Close();
            }

            return t;
        }

        static void Main(string[] args)
        {
            Tablero t = new Tablero(9, 9, 10);
            bool bomba = false;
            char input = ' ';
            string ent;

            Console.WriteLine("BUSCAMINAS");
            Console.WriteLine();
            Console.WriteLine("1. Nueva Partida");
            Console.WriteLine("2. Continuar Partida");
            Console.WriteLine();
            Console.Write("Escribe una opción (1 o 2): ");

            ent = Console.ReadLine();

            if (ent == "1")
            {
                Juego(t, input);
            }

            else if (ent == "2")
            {
                Console.Write("Escribe el nombre del archivo de guardado de la partida: ");
                string nombre = Console.ReadLine();

                Tablero t2 = RecuperaPartida(nombre);

                if (t2 != null)
                {
                    Juego(t2, input);
                }
                else Console.WriteLine("No se pudo recuperar la partida");
                
            }

            else Console.WriteLine("Error : introduce 1 o 2");


        }
    }
}
