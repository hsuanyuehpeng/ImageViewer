﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextureViewer.Models
{
    /// <summary>
    /// container for all models
    /// </summary>
    public class Models
    {
        /// <summary>
        /// window after the opengl host was initialized
        /// </summary>
        /// <param name="app"></param>
        /// <param name="window"></param>
        public Models(App app, MainWindow window)
        {
            this.App = new AppModel(app, window);
            GlContext = new OpenGlContext(window);
            GlContext.Enable();

            Images = new ImagesModel(GlContext);
            GlData = new OpenGlModel(GlContext, Images);
            Display = new DisplayModel(Images, GlContext);
            Equations = new ImageEquationsModel(Images);
            Progress = new ProgressModel();
            FinalImages = new FinalImagesModel(GlData.TextureCache, Images);

            GlContext.Disable();
        }

        public void Dispose()
        {
            GlContext.Enable();

            FinalImages.Dispose();
            Images.Dispose();
            GlData.Dispose();

            GlContext.Disable();
        }

        public OpenGlContext GlContext { get; }
        public OpenGlModel GlData { get; }
        public ImagesModel Images { get; }
        public DisplayModel Display { get; }
        public AppModel App { get; }
        public ImageEquationsModel Equations { get; }
        public ProgressModel Progress { get; }
        public FinalImagesModel FinalImages { get; }
    }
}