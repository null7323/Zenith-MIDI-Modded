using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZenithEngine;
using OpenTK.Graphics.OpenGL;

namespace Zenith_MIDI
{
    class GLPostbuffer: IDisposable
    {
        public int BufferID;// { get; private set; }
        public int TextureID;// { get; private set; }

        public GLPostbuffer(int width, int height)
        {
            int b, t;
            GLUtils.GenFrameBufferTexture(width, height, out b, out t);
            BufferID = b;
            TextureID = t;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void BindBuffer()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, BufferID);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void BindTexture()
        {
            GL.BindTexture(TextureTarget.Texture2D, TextureID);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void UnbindBuffers()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void UnbindTextures()
        {
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            GL.DeleteFramebuffer(BufferID);
            GL.DeleteTexture(TextureID);
        }
    }
}
