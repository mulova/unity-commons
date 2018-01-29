﻿using System;
using UnityEditor;
using Object = UnityEngine.Object;
using UnityEngine;
using System.IO;
using commons;
using System.Collections.Generic;

namespace comunity
{
    public static class TextureUtil
    {
        public static string IMAGE_MAGICK
        {
            get
            {
                return Environment.GetEnvironmentVariable("IMAGE_MAGICK");
            }
        }
        
        public static bool IsValid()
        {
            return IMAGE_MAGICK.IsNotEmpty();
        }
        
        private static ExecOutput Exec(string cmd, string src, string options)
        {
            string dir = PathUtil.GetDirectory(src);
            string filename = Path.GetFileName(src);
            string dst = PathUtil.Combine(dir, "tmp_"+filename);
            var output = Exec(cmd, src, dst, options);
            File.Delete(src);
            File.Delete(dst+".meta");
            File.Move(dst, src);
            AssetDatabase.Refresh();
            AssetDatabase.ImportAsset(src, ImportAssetOptions.ForceUpdate);
            return output;
        }
        
        public static ExecOutput Exec(string cmd, string[] src, string dst, string options)
        {
            string srcStr = src.Convert(s=>s.Wrap("\"")).Join(" ");
            string param = string.Format("{0} {1} \"{2}\"", options, srcStr, dst);
            var result = Exec(cmd, param);
            AssetDatabase.ImportAsset(dst, ImportAssetOptions.ForceUpdate);
            return result;
        }
        
        public static ExecOutput Exec(string cmd, string src, string dst, string options)
        {
            return Exec(cmd, new string[] {src}, dst, options);
        }
        
        public static ExecOutput Exec(string cmd, string param)
        {
            if (IMAGE_MAGICK.IsEmpty())
            {
                throw new Exception("environment variable 'IMAGE_MAGICK' is not set");
            }
            string command = null;
            if (File.Exists(IMAGE_MAGICK))
            {
                command = IMAGE_MAGICK;
                param = string.Concat(cmd, " ", param);
            } else
            {
                command = PathUtil.Combine(IMAGE_MAGICK, cmd);
            }
            ExecOutput output = EditorUtil.ExecuteCommand(command, param, System.Text.Encoding.UTF8);
            if (output.IsError())
            {
                throw new Exception(output.stderr);
            }
            return output;
        }
        
        public static string ConvertType(string src, string ext, string options)
        {
            string dst = PathUtil.ReplaceExtension(src, ext);
            Exec("convert", src, dst, options);
            return dst;
        }
        
        public static string ToJpeg(string src)
        {
            return ConvertType(src, ".jpg", string.Empty);
        }
        
        public static string ToPng(string src)
        {
            return ConvertType(src, ".png", "-flatten");
        }
        
        public static void Dither4444(Texture2D tex)
        {
            if (tex == null)
            {
                return;
            }
            int texw = tex.width;
            int texh = tex.height;
            
            Color[] pixels = tex.GetPixels();
            int offs = 0;
            
            float k1Per15 = 1.0f / 15.0f;
            float k1Per16 = 1.0f / 16.0f;
            float k3Per16 = 3.0f / 16.0f;
            float k5Per16 = 5.0f / 16.0f;
            float k7Per16 = 7.0f / 16.0f;
            
            for (int y = 0; y < texh; y++)
            {
                for (int x = 0; x < texw; x++)
                {
                    float a = pixels[offs].a;
                    float r = pixels[offs].r;
                    float g = pixels[offs].g;
                    float b = pixels[offs].b;
                    
                    float a2 = Mathf.Clamp01(Mathf.Floor(a * 16) * k1Per15);
                    float r2 = Mathf.Clamp01(Mathf.Floor(r * 16) * k1Per15);
                    float g2 = Mathf.Clamp01(Mathf.Floor(g * 16) * k1Per15);
                    float b2 = Mathf.Clamp01(Mathf.Floor(b * 16) * k1Per15);
                    
                    float ae = a-a2;
                    float re = r-r2;
                    float ge = g-g2;
                    float be = b-b2;
                    
                    pixels[offs].a = a2;
                    pixels[offs].r = r2;
                    pixels[offs].g = g2;
                    pixels[offs].b = b2;
                    
                    int n1 = offs+1;
                    int n2 = offs+texw-1;
                    int n3 = offs+texw;
                    int n4 = offs+texw+1;
                    
                    if (x < texw-1)
                    {
                        pixels[n1].a += ae * k7Per16;
                        pixels[n1].r += re * k7Per16;
                        pixels[n1].g += ge * k7Per16;
                        pixels[n1].b += be * k7Per16;
                    }
                    
                    if (y < texh-1)
                    {
                        pixels[n3].a += ae * k5Per16;
                        pixels[n3].r += re * k5Per16;
                        pixels[n3].g += ge * k5Per16;
                        pixels[n3].b += be * k5Per16;
                        
                        if (x > 0)
                        {
                            pixels[n2].a += ae * k3Per16;
                            pixels[n2].r += re * k3Per16;
                            pixels[n2].g += ge * k3Per16;
                            pixels[n2].b += be * k3Per16;
                        }
                        
                        if (x < texw-1)
                        {
                            pixels[n4].a += ae * k1Per16;
                            pixels[n4].r += re * k1Per16;
                            pixels[n4].g += ge * k1Per16;
                            pixels[n4].b += be * k1Per16;
                        }
                    }
                    
                    offs++;
                }
            }
            
            tex.SetPixels(pixels);
            EditorUtility.CompressTexture(tex, TextureFormat.RGBA4444, TextureCompressionQuality.Best);
        }
        
        public static void FloydSteinberg4444(Texture tex)
        {
            if (tex == null)
            {
                return;
            }
            string src = AssetDatabase.GetAssetPath(tex);
            string dst = PathUtil.AddFileSuffix(src, "_4444");
            File.Copy(src, dst, false);
            
            Exec("convert", src, dst, "-quantize transparent -dither FloydSteinberg -colors 255");
            AssetDatabase.ImportAsset(dst, ImportAssetOptions.ForceSynchronousImport);
        }
        
        /**
        * @return path of 'RGB', 'A' texture
        */
        public static string[] SplitAlphaChannel(Texture tex, bool genAlpha)
        {
            if (tex == null)
            {
                return null;
            }
            string src = AssetDatabase.GetAssetPath(tex);
            string rgb = PathUtil.ReplaceExtension(src, ".jpg");
            if (File.Exists(rgb))
            {
                AssetDatabase.DeleteAsset(rgb);
            }
            Exec("convert", src, rgb, null);
            if (genAlpha)
            {
                string a = PathUtil.AddFileSuffix(src, "_a");
                if (File.Exists(a))
                {
                    AssetDatabase.DeleteAsset(a);
                }
                Exec("convert", src, a, "-set colorspace RGBA -alpha extract");
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                return new string[] { rgb, a };
            } else
            {
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                return new string[] { rgb };
            }
        }
        
        public static string[] SplitChannel4ETC(Texture tex)
        {
            if (tex == null)
            {
                return null;
            }
            return SplitChannel4ETC(tex, TextureImporterFormat.Alpha8);
        }
        
        public static string[] SplitChannel4ETC(Texture tex, TextureImporterFormat alphaFormat)
        {
            if (tex == null)
            {
                return null;
            }
            string src = AssetDatabase.GetAssetPath(tex);
            string rgb = PathUtil.ReplaceExtension(src, ".jpg");
            if (File.Exists(rgb))
            {
                AssetDatabase.DeleteAsset(rgb);
            }
            Exec("convert", src, rgb, null); // v1
//          Exec("convert", src, rgb, "-background \"#000000\" -alpha remove");
            AssetDatabase.ImportAsset(rgb, ImportAssetOptions.ForceSynchronousImport);
            CopyTextureImporterSettings(src, rgb);
            string a = PathUtil.AddFileSuffix(src, "_a");
            if (File.Exists(a))
            {
                AssetDatabase.DeleteAsset(a);
            }
            if (alphaFormat == TextureImporterFormat.Alpha8)
            {
                //          EditorAssetUtil.SplitTexAlpha(src, rgb, a);
                //          Exec("convert", src, a, "-set colorspace RGBA -alpha extract");
                Exec("convert", src, a, "-channel A -alpha extract -alpha copy");
            } else
            {
                Exec("convert", src, a, "-channel A -separate");
            }
            AssetDatabase.ImportAsset(a, ImportAssetOptions.ForceSynchronousImport);
            CopyTextureImporterSettings(src, a);
            
            // set texture property
            TextureImporter rgbImp = OptimizeTexture(rgb);
            TextureImporter aImp = OptimizeTexture(a, alphaFormat);
            if (!AssetConfig.TEX_NPOT && !tex.IsPOT())
            {
                rgbImp.npotScale = TextureImporterNPOTScale.ToNearest;
                aImp.npotScale = TextureImporterNPOTScale.ToNearest;
            }

            int texSize = Math.Max(tex.width, tex.height);
            int invScale = AssetLabel.ALPHA_SD.Is(tex)? 2 : 1;
            aImp.maxTextureSize = texSize / invScale;
            rgbImp.SaveAndReimport();
            aImp.SaveAndReimport();
            AssetDatabase.ImportAsset(rgb, ImportAssetOptions.ForceSynchronousImport);
            AssetDatabase.ImportAsset(a, ImportAssetOptions.ForceSynchronousImport);
            
            return new string[] { rgb, a };
        }
        
        public static TextureImporter CopyTextureImporterSettings(string src, string dst, IEnumerable<string> platforms = null)
        {
            TextureImporter srcIm = AssetImporter.GetAtPath(src) as TextureImporter;
            TextureImporter dstIm = AssetImporter.GetAtPath(dst) as TextureImporter;
            TextureImporterSettings imSetting = new TextureImporterSettings();
            srcIm.ReadTextureSettings(imSetting);
            imSetting.textureType = TextureImporterType.Default;
            imSetting.alphaSource = TextureFormatEx.Get(dst).HasAlpha()? TextureImporterAlphaSource.FromInput : TextureImporterAlphaSource.None;
            dstIm.SetTextureSettings(imSetting);
            if (platforms != null)
            {
                foreach (var p in platforms)
                {
                    var s = srcIm.GetPlatformTextureSettings(p);
                    dstIm.SetPlatformTextureSettings(s);
                }
            }
            dstIm.SaveAndReimport();
            return dstIm;
        }

        public static TextureImporter OptimizeTexture(string path, bool reimport = true)
        {
            TextureImporter im = AssetImporter.GetAtPath(path) as TextureImporter;
            im.anisoLevel = 1;
            im.isReadable = false;
            im.compressionQuality = 1;
            im.spriteImportMode = SpriteImportMode.None;
            im.mipmapEnabled = false;
            if (reimport)
            {
                im.SaveAndReimport();
            }
            return im;
        }
        
        public static TextureImporter OptimizeTexture(string path, TextureImporterFormat texFormat)
        {
            TextureImporter im = OptimizeTexture(path, false);
            foreach (PlatformId p in PlatformId.PLATFORMS)
            {
                TextureImporterPlatformSettings s = im.GetPlatformTextureSettings(p);
                s.overridden = true;
                s.format = texFormat;
                im.SetPlatformTextureSettings(s);
            }
            im.SaveAndReimport();
            return im;
        }
        
        public static string AppendVertically(string tex1, string tex2)
        {
            string dst = PathUtil.AddFileSuffix(tex1, "_vert");
            Exec("convert", new string[]{ tex1, tex2 }, dst, "-append");
            CopyTextureImporterSettings(tex1, dst);
            OptimizeTexture(dst, TextureImporterFormat.Automatic);
            AssetDatabase.DeleteAsset(tex1);
            AssetDatabase.DeleteAsset(tex2);
            return dst;
        }
        
        public static string AppendHorizontally(string tex1, string tex2)
        {
            string dst = PathUtil.AddFileSuffix(tex1, "_horiz");
            Exec("convert", new string[]{ tex1, tex2 }, dst, "+append");
            CopyTextureImporterSettings(tex1, dst);
            OptimizeTexture(dst, TextureImporterFormat.Automatic);
            AssetDatabase.DeleteAsset(tex1);
            AssetDatabase.DeleteAsset(tex2);
            return dst;
        }
        
        public static void ScaleToImporterSizeRespectRatio(Texture t)
        {
            string src = AssetDatabase.GetAssetPath(t);
            Exec("convert", src, string.Format("-resize {0}x{1}", t.width, t.height));
            AssetDatabase.ImportAsset(src, ImportAssetOptions.ForceUpdate);
        }
        
        public static void ScaleToImporterSize(Texture t)
        {
            string src = AssetDatabase.GetAssetPath(t);
            Exec("convert", src, string.Format("-resize {0}x{1}\\!", t.width, t.height));
            AssetDatabase.ImportAsset(src, ImportAssetOptions.ForceUpdate);
        }
        
        public static void Scale(Texture t, float scale)
        {
            string src = AssetDatabase.GetAssetPath(t);
            Exec("convert", src, string.Format("-resize {0:N2}%", scale * 100));
            AssetDatabase.ImportAsset(src, ImportAssetOptions.ForceUpdate);
        }
        
        public static void Resize(Texture t, int width, int height)
        {
            string src = AssetDatabase.GetAssetPath(t);
            Exec("convert", src, string.Format("-resize {0}x{1}\\!", width, height));
            AssetDatabase.ImportAsset(src, ImportAssetOptions.ForceUpdate);
        }
    }
}