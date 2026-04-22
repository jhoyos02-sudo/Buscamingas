// Javier Hoyos Giunta
// Hector Prous Arroyo
using System.Drawing;
using System.Reflection.Metadata;
using Coordinates;
using SetArray;

namespace Buscamingas
{
    internal class Program
    {
        class Tablero
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
                this.cursor = new Coor(0, 0);

                casilla = new Casilla[fils,cols];
                for (int i = 0; i < fils; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        casilla[i, j].estado = 'o';
                        casilla[i,j].mina = false;
                    }
                }
                ponMinas1(numMinas);
            }

            private void ponMinas1(int nMinas)
            {
                int i = 0;
                while (i < nMinas)
                {
                    int x = rnd.Next(0, fils);
                    int y = rnd.Next(0, cols);

                    if (casilla[x, y].mina == false)
                    {
                        casilla[x,y].mina = true;
                        i++;
                    }
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
                debug = true;
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

                        if (debug || bomba)
                        {
                            if (casilla[i, j].mina && casilla[i, j].estado != 'x')
                                Console.Write('M');
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
                if (casilla[cursor.X, cursor.Y].estado == 'o')
                {
                    casilla[cursor.X, cursor.Y].estado = '*';
                    nMarcadas++;
                }
                else if (casilla[cursor.X, cursor.Y].estado == '*') 
                {
                    casilla[cursor.X, cursor.Y].estado = 'o'; 
                    nMarcadas--;
                }

            }

            public bool ClickCasilla()
            {
                if (casilla[cursor.X, cursor.Y].estado == 'o')
                {
                    if (casilla[cursor.X, cursor.Y].mina)
                    {
                        casilla[cursor.X, cursor.Y].estado = 'x';
                    }
                    else
                    {
                        int minas = MinasAlrededor(cursor.X, cursor.Y);

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

                return numMinas;
            }

            private void DescubreAdyacentes()
            {
                // importante hacer una separación entre visitadas y pendientes (con dos arrays de posiciones)

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

                                if (!visitadas.Belongs(ady) && !pendientes.Belongs(ady))
                                {
                                    pendientes.Add(ady);

                                    if (MinasAlrededor(i, j) == 0)
                                    {
                                        casilla[i, j].estado = '.';
                                    }
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
                        if (!casilla[i,j].mina && casilla[i,j].estado == 'o')
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
        }

        bool ProcesaInput(Tablero t, char c) 
        {
            Coor up, down, left, right;

            left = new Coor(-1, 0);
            right = new Coor(1, 0);
            up = new Coor(0, 1);
            down = new Coor(0, -1);
            
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
                    t.ClickCasilla();
                    break;
            }
            return t.ClickCasilla();
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
            }
            while (Console.KeyAvailable) Console.ReadKey().Key.ToString();
            return d;
        }

        static void Main(string[] args)
        {
            Tablero t = new Tablero(9, 9, 10);
            bool bomba = false;
            char input = ' ';
            t.activateDEBUG();

            Coor left = new Coor(0, -1);
            Coor right = new Coor(0, 1);
            Coor up = new Coor(-1, 0);
            Coor down = new Coor(1, 0);

            t.Render(false);

            while (input != 'q' && !bomba && !t.Terminado())
            {
                input = LeeInput();

                if (input == 'l') t.MueveCursor(left);
                else if (input == 'r') t.MueveCursor(right);
                else if (input == 'u') t.MueveCursor(up);
                else if (input == 'd') t.MueveCursor(down);
                else if (input == 'x') t.MarcaMina();
                else if (input == 'c') bomba = t.ClickCasilla();

                t.Render(bomba);
            }

            if (bomba)
                Console.WriteLine("Has perdido!");
            else if (t.Terminado())
                Console.WriteLine("Has ganado!");
            else
                Console.WriteLine("Has abandonado!");
        }
    }
}
