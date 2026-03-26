using System.Drawing;
using Coordinates;

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

            }

            public void MueveCursor(Coor dir)
            {
                //cursor += dir;  (esto si hacemos una sobrecarga del operador += en la clase coor)
            }

            public void MarcaMina()
            {

            }

            public bool ClickCasilla()
            {
                
                return true;
            }

            private int MinasAlrededor(int x, int y)
            {
                // usar clamp
                return 0;
            }

            private void DescubreAdyacentes()
            {
                // importante hacer una separación entre visitadas y pendientes (con dos arrays de posiciones)
            }

            public bool Terminado()
            {

                return true;
            }
        }

        bool ProcesaInput(Tablero t, char c) 
        {

            return true;
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
            Console.WriteLine("Hello, World!");
        }
    }
}
