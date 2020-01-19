﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.ImageLoader;
using ImageFramework.Model;
using ImageFramework.Model.Scaling;
using ImageFramework.Model.Scaling.Down;
using ImageFramework.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX.DXGI;

namespace FrameworkTests.Model.Scaling
{
    [TestClass]
    public class DownscalingTest
    {
        [TestMethod]
        public void FastScaling()
        {
            var models = new Models(1);
            models.AddImageFromFile(TestData.Directory + "checkers_wide.png");
            models.Pipelines[0].Color.Formula = "I0 + 0"; // trick pipeline to create a rgba32 target for us
            models.Apply();

            var src = models.Pipelines[0].Image;
            Assert.IsNotNull(src);
            Assert.AreEqual(SharpDX.DXGI.Format.R32G32B32A32_Float ,src.Format);
            var mipped = src.GenerateMipmapLevels(src.Size.MaxMipLevels, false);
            // mip levels:
            // 0: 8 x 4
            // 1: 4 x 2
            // 2: 2 x 1
            // 3: 1 x 1

            models.Scaling.WriteMipmaps(mipped);

            // test mipmaps
            var mip1 = mipped.GetPixelColors(0, 1);
            var refMip1 = IO.LoadImageTexture(TestData.Directory + "checkers_wide_mip1.png").GetPixelColors(0, 0);

            Console.Out.WriteLine("Testing fast results");

            TestData.CompareColors(refMip1, mip1);

            var mip2 = mipped.GetPixelColors(0, 2);
            Assert.AreEqual(2, mip2.Length);
            Assert.IsTrue(new Color(0.5f).Equals(mip2[0], Color.Channel.Rgb));

            Console.Out.WriteLine("Testing copy results");

            var mip3 = mipped.GetPixelColors(0, 3);
            Assert.AreEqual(1, mip3.Length);
            Assert.IsTrue(new Color(0.5f).Equals(mip3[0], Color.Channel.Rgb));
        }

        [TestMethod]
        public void FastSlowScaling()
        {
            var models = new Models(1);
            models.AddImageFromFile(TestData.Directory + "checkers3x7.png");
            models.Pipelines[0].Color.Formula = "I0 + 0"; // trick pipeline to create a rgba32 target for us
            models.Apply();

            var src = models.Pipelines[0].Image;
            Assert.IsNotNull(src);
            Assert.AreEqual(SharpDX.DXGI.Format.R32G32B32A32_Float, src.Format);
            var mipped = src.GenerateMipmapLevels(src.Size.MaxMipLevels, false);
            // mip levels:
            // 0: 3 x 7
            // 1: 1 x 3 fast + slow
            // 2: 1 x 1 fast + fast

            models.Scaling.WriteMipmaps(mipped);

            // test mipmaps
            var mip1 = mipped.GetPixelColors(0, 1);
            var refMip1 = IO.LoadImageTexture(TestData.Directory + "checkers3x7_mip1.png").GetPixelColors(0, 0);
            TestData.CompareColors(refMip1, mip1);

            var mip2 = mipped.GetPixelColors(0, 2);
            Assert.AreEqual(1, mip2.Length);
            Assert.IsTrue(new Color(0.476f).Equals(mip2[0], Color.Channel.Rgb));
        }

        [TestMethod]
        public void Scaling3D()
        {
            var models = new Models(1);
            models.AddImageFromFile(TestData.Directory + "checkers3d.dds");
            models.Pipelines[0].Color.Formula = "I0 + 0"; // trick pipeline to create a rgba32 target for us
            models.Apply();

            var original = models.Images.Images[0].Image;
            var dst = models.Pipelines[0].Image;
            Assert.IsNotNull(dst);
            Assert.AreEqual(SharpDX.DXGI.Format.R32G32B32A32_Float, dst.Format);
            Assert.IsTrue(dst != models.Images.Images[0].Image);

            // overwrite mipmaps
            models.Scaling.WriteMipmaps(dst);

            TestData.CompareColors(original.GetPixelColors(0, 1), dst.GetPixelColors(0, 1));
            TestData.CompareColors(original.GetPixelColors(0, 2), dst.GetPixelColors(0, 2));
        }

        [TestMethod]
        public void BoxCompile()
        {
            var s = new BoxScalingShader();
            s.CompileShaders();
        }

        [TestMethod]
        public void TriangleCompile()
        {
            var s = new TriangleScalingShader();
            s.CompileShaders();
        }

        [TestMethod]
        public void LanzosCompile()
        {
            var s = new LanzosScalingShader();
            s.CompileShaders();
        }

        [TestMethod]
        public void DetailPreservingCompile()
        {
            var s = new DetailPreservingDownscalingShader(null, true);
            s.CompileShaders();
        }

        [TestMethod]
        public void CompileDetailPreservingCore()
        {
            var s = new DetailPreservingShaderCore(true);
            s.CompileShaders();
            s = new DetailPreservingShaderCore(false);
            s.CompileShaders();
        }

        [TestMethod]
        public void TriangleCheckersTest()
        {
            // the results should be identical with the box filter in this case
            var models = new Models(1);
            models.AddImageFromFile(TestData.Directory + "checkers.dds");
            models.Pipelines[0].Color.Formula = "I0 + 0"; // trick pipeline to create a rgba32 target for us
            models.Apply();

            var src = models.Pipelines[0].Image;
            Assert.IsNotNull(src);
            Assert.AreEqual(SharpDX.DXGI.Format.R32G32B32A32_Float, src.Format);
            var mipped = src.CloneWithoutMipmaps().GenerateMipmapLevels(src.Size.MaxMipLevels, false);
            // mip levels:
            // 0: 4 x 4
            // 1: 2 x 2
            // 2: 1 x 1

            models.Scaling.Minify = ScalingModel.MinifyFilters.Triangle;
            models.Scaling.WriteMipmaps(mipped);

            // test mipmaps
            var mip1 = mipped.GetPixelColors(0, 1);
            TestData.TestCheckersLevel1(mip1);

            var mip2 = mipped.GetPixelColors(0, 2);
            TestData.TestCheckersLevel2(mip2);
        }

        [TestMethod]
        public void FastGaussTest()
        {
            // filter kernel:
            // 1 2 1
            // 2 4 2
            // 1 2 1

            var s = new FastGaussShader();
            var img = IO.LoadImageTexture(TestData.Directory + "small.pfm");
            var dst = new TextureArray2D(1, 1, img.Size, Format.R32G32B32A32_Float, true);

            s.Run(img, dst, 0, false, new UploadBuffer(256));

            var src = img.GetPixelColors(0, 0);
            var res = dst.GetPixelColors(0, 0);
            Assert.AreEqual(3 * 3, res.Length);

            // expected values calculated by hand
            float midR = (src[0].Red + src[1].Red * 2.0f + src[2].Red
                           + src[3].Red * 2.0f + src[4].Red * 4.0f + src[5].Red * 2.0f
                           + src[6].Red + src[7].Red * 2.0f + src[8].Red) / 16.0f;
            float midG = (src[0].Green + src[1].Green * 2.0f + src[2].Green
                          + src[3].Green * 2.0f + src[4].Green * 4.0f + src[5].Green * 2.0f
                          + src[6].Green + src[7].Green * 2.0f + src[8].Green) / 16.0f;
            var midColor = new Color(midR, midG, 1.0f);

            float firstR = (src[0].Red * 4.0f + src[1].Red * 2.0f
                            + src[3].Red * 2.0f + src[4].Red) / 9.0f;
            var firstColor = new Color(firstR);

            Assert.IsTrue(res[4].Equals(midColor, Color.Channel.R | Color.Channel.G));
            Assert.IsTrue(res[0].Equals(firstColor, Color.Channel.R));
        }
    }
}
