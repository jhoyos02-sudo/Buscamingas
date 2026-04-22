//Enrique Satrústegui Coca
//Jesus Maldonado Becerra

using Coordinates;
namespace SetArray
{
    class SetCoor
    {
        // atributos de la clase
        private Coor[] coors; // array con coordenadas
        private int oc; // núm de componentes ocupadas del array = primera pos libre

        public SetCoor(int tam = 10)
        {
            coors = new Coor[tam];
            oc = 0;
        }

        private int SearchElem(Coor c)
        {
            int i = 0;
            bool encontrado = false;

            while (i < oc && !encontrado)
            {
                encontrado = c == coors[i];
                if (!encontrado)
                {
                    i++;
                }
            }
            if (!encontrado) i = -1;

            return i;
        }

        public bool Add(Coor c)
        {
            bool added = false;
           
            if (!Belongs(c))
            {
                if (oc < coors.Length - 1)
                {
                    coors[oc] = c;
                    oc++;
                    added = true;
                }
                else Console.WriteLine("Array lleno");
            } 

            return added;
        }
        public bool Belongs(Coor c)
        {
            return SearchElem(c) != -1;
        }

        public Coor PopElem()
        {
            Coor c = coors[oc - 1];
            oc--;
            return c;
        }

        public int GetOc()
        {
            return oc;
        }
    }
}
