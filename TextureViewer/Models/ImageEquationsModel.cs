﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TextureViewer.Models
{
    /// <summary>
    /// container for the image equations
    /// </summary>
    public class ImageEquationsModel
    {
        private readonly ImageEquationModel[] equations;

        public int NumEquations => equations.Length;

        public ImageEquationsModel(ImagesModel images)
        {
            equations = new ImageEquationModel[2]
            {
                new ImageEquationModel(true, 0, images),
                new ImageEquationModel(false, 1, images)
            };
        }

        public ImageEquationModel Get(int id)
        {
            Debug.Assert(id >= 0 && id < NumEquations);
            return equations[id];
        }

        /// <summary>
        /// returns the ids of all visible equations
        /// </summary>
        /// <returns></returns>
        public List<int> GetVisibles()
        {
            var res = new List<int>();
            for (var i = 0; i < equations.Length; ++i)
            {
                if(equations[i].Visible)
                    res.Add(i);
            }

            return res;
        }
    }
}