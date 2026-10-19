using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts.Base_Assembly.Data.Data_Classes.Grid.New.World.Buildings
{
    public class ResearchProd : IAssign /*IProduction*/
    {
        AssignData data;
        public AssignData AssignData { get => data; set => data = value; }
        
        /*public float ProdTime { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public float CurrentTime { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool Stoped { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ModifiableFloat ProdSpeed { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Product()
        {
            throw new NotImplementedException();
        }*/
    }
}
