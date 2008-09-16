// AForge Image Processing Library
// AForge.NET framework
//
// Copyright � Andrew Kirillov, 2005-2008
// andrew.kirillov@gmail.com
//

namespace AForge.Imaging
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using AForge.Math;

    /// <summary>
    /// Vertical intensity statistics.
    /// </summary>
    /// 
    /// <remarks><para>The class provides information about vertical distribution
    /// of pixel intensities, which may be used to locate objects, their centers, etc.
    /// </para>
    /// 
    /// <para>The class accepts grayscale (8 bpp indexed) and color (24 bpp) images.</para>
    /// 
    /// <para>Sample usage:</para>
    /// <code>
    /// // collect statistics
    /// VerticalIntensityStatistics vis = new VerticalIntensityStatistics( sourceImage );
    /// // get gray histogram (for grayscale image)
    /// Histogram histogram = vis.Gray;
    /// // output some histogram's information
    /// System.Diagnostics.Debug.WriteLine( "Mean = " + histogram.Mean );
    /// System.Diagnostics.Debug.WriteLine( "Min = " + histogram.Min );
    /// System.Diagnostics.Debug.WriteLine( "Max = " + histogram.Max );
    /// </code>
    /// 
    /// <para><b>Sample grayscale image with its vertical intensity histogram:</b></para>
    /// <img src="img/imaging/ver_histogram.jpg" width="450" height="240" />
    /// </remarks>
    /// 
    /// <seealso cref="HorizontalIntensityStatistics"/>
    ///
    public class VerticalIntensityStatistics
    {
        // histograms for RGB channgels
        private Histogram red   = null;
        private Histogram green = null;
        private Histogram blue  = null;
        // grayscale histogram
        private Histogram gray  = null;

        /// <summary>
        /// Histogram for red channel.
        /// </summary>
        /// 
        public Histogram Red
        {
            get { return red; }
        }

        /// <summary>
        /// Histogram for green channel.
        /// </summary>
        /// 
        public Histogram Green
        {
            get { return green; }
        }

        /// <summary>
        /// Histogram for blue channel.
        /// </summary>
        /// 
        public Histogram Blue
        {
            get { return blue; }
        }

        /// <summary>
        /// Histogram for gray channel (intensities).
        /// </summary>
        /// 
        public Histogram Gray
        {
            get { return gray; }
        }

        /// <summary>
        /// Value wich specifies if the processed image was color or grayscale.
        /// </summary>
        /// 
        /// <remarks><para>If the property equals to <b>true</b>, then the <see cref="Gray"/>
        /// property should be used to retrieve histogram for the processed grayscale image.
        /// Otherwise <see cref="Red"/>, <see cref="Green"/> and <see cref="Blue"/> property
        /// should be used to retrieve histogram for particular RGB channel of the processed
        /// color image.</para></remarks>
        /// 
        public bool IsGrayscale
        {
            get { return ( gray == null ); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VerticalIntensityStatistics"/> class.
        /// </summary>
        /// 
        /// <param name="image">Source image.</param>
        ///
        /// <exception cref="ArgumentException">Unsupported pixel format of the source image.</exception>
        /// 
        public VerticalIntensityStatistics( Bitmap image )
        {
            // check image format
            if (
                ( image.PixelFormat != PixelFormat.Format8bppIndexed ) &&
                ( image.PixelFormat != PixelFormat.Format24bppRgb )
                )
            {
                throw new ArgumentException( "Unsupported pixel format of the source image." );
            }

            // lock bitmap data
            BitmapData imageData = image.LockBits(
                new Rectangle( 0, 0, image.Width, image.Height ),
                ImageLockMode.ReadOnly, image.PixelFormat );

            // gather statistics
            ProcessImage( new UnmanagedImage( imageData ) );

            // unlock image
            image.UnlockBits( imageData );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VerticalIntensityStatistics"/> class.
        /// </summary>
        /// 
        /// <param name="imageData">Source image data.</param>
        ///
        /// <exception cref="ArgumentException">Unsupported pixel format of the source image.</exception>
        /// 
        public VerticalIntensityStatistics( BitmapData imageData )
            : this( new UnmanagedImage( imageData ) )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HorizontalIntensityStatistics"/> class.
        /// </summary>
        /// 
        /// <param name="image">Source unmanaged image.</param>
        /// 
        /// <exception cref="ArgumentException">Unsupported pixel format of the source image.</exception>
        /// 
        public VerticalIntensityStatistics( UnmanagedImage image )
        {
            // check image format
            if (
                ( image.PixelFormat != PixelFormat.Format8bppIndexed ) &&
                ( image.PixelFormat != PixelFormat.Format24bppRgb )
                )
            {
                throw new ArgumentException( "Unsupported pixel format of the source image." );
            }

            // gather statistics
            ProcessImage( image );
        }

        /// <summary>
        /// Gather vertical intensity statistics for specified image.
        /// </summary>
        /// 
        /// <param name="image">Source image.</param>
        /// 
        private void ProcessImage( UnmanagedImage image )
        {
            // get image dimension
            int width  = image.Width;
            int height = image.Height;

            // do the job
            unsafe
            {
                byte* p = (byte*) image.ImageData.ToPointer( );

                // check pixel format
                if ( image.PixelFormat == PixelFormat.Format8bppIndexed )
                {
                    int offset = image.Stride - width;

                    // histogram array
                    int[] g = new int[height];

					// for each pixel
                    for ( int y = 0; y < height; y++ )
                    {
                        int lineSum = 0;

                        // for each pixel
                        for ( int x = 0; x < width; x++, p++ )
                        {
                            lineSum += *p;
                        }
                        g[y] = lineSum;

                        p += offset;
                    }

                    // create historgram for gray level
                    gray = new Histogram( g );
                }
                else
                {
                    int offset = image.Stride - width * 3;

                    // histogram arrays
                    int[] r = new int[height];
                    int[] g = new int[height];
                    int[] b = new int[height];

                    // for each line
                    for ( int y = 0; y < height; y++ )
                    {
                        int lineRSum = 0;
                        int lineGSum = 0;
                        int lineBSum = 0;

                        // for each pixel
                        for ( int x = 0; x < width; x++, p += 3 )
                        {
                            lineRSum += p[RGB.R];
                            lineGSum += p[RGB.G];
                            lineBSum += p[RGB.B];
                        }
                        r[y] = lineRSum;
                        g[y] = lineGSum;
                        b[y] = lineBSum;

                        p += offset;
                    }

                    // create histograms
                    red   = new Histogram( r );
                    green = new Histogram( g );
                    blue  = new Histogram( b );
                }
            }
        }
    }
}
