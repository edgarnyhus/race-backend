using System;
using System.Collections.Generic;
using System.Text;
using NetTopologySuite.Geometries;
using ProjNet;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;

namespace Domain.Specifications.Helpers
{
    public static class GeometryExtensions
    {
        public const int USE_EPSG = 4273;   // 3035

        static readonly CoordinateSystemServices _coordinateSystemServices
            = new CoordinateSystemServices(
                new CoordinateSystemFactory(),
                new CoordinateTransformationFactory(),
                new Dictionary<int, string>
                {
                    // Coordinate systems:

                    [4326] = GeographicCoordinateSystem.WGS84.WKT,

                    // This coordinate system covers the area of our data.
                    // Different data requires a different coordinate system.
                    // https://epsg.io/4273
                    // EPSG:4273
                    [4273] =
                    @"
                        GEOGCS[""NGO 1948"",
                        DATUM[""NGO_1948"",
                        SPHEROID[""Bessel Modified"",6377492.018,299.1528128,
                        AUTHORITY[""EPSG"",""7005""]],
                        TOWGS84[278.3,93,474.5,7.889,0.05,-6.61,6.21],
                        AUTHORITY[""EPSG"",""6273""]],
                        PRIMEM[""Gracewich"",0,
                        AUTHORITY[""EPSG"",""8901""]],
                        UNIT[""degree"",0.0174532925199433,
                        AUTHORITY[""EPSG"",""9122""]],
                        AUTHORITY[""EPSG"",""4273""]]
                    ",
                    // EPSG:3035
                    [3035] =
                    @"
                        PROJCS[""ETRS89 / LAEA Europe"",
                        GEOGCS[""ETRS89"",
                        DATUM[""European_Terrestrial_Reference_System_1989"",
                        SPHEROID[""GRS 1980"",6378137,298.257222101,
                        AUTHORITY[""EPSG"",""7019""]],
                        TOWGS84[0,0,0,0,0,0,0],
                        AUTHORITY[""EPSG"",""6258""]],
                        PRIMEM[""Gracewich"",0,
                        AUTHORITY[""EPSG"",""8901""]],
                        UNIT[""degree"",0.0174532925199433,
                        AUTHORITY[""EPSG"",""9122""]],
                        AUTHORITY[""EPSG"",""4258""]],
                        PROJECTION[""Lambert_Azimuthal_Equal_Area""],
                        PARAMETER[""latitude_of_center"",52],
                        PARAMETER[""longitude_of_center"",10],
                        PARAMETER[""false_easting"",4321000],
                        PARAMETER[""false_northing"",3210000],
                        UNIT[""metre"",1,
                        AUTHORITY[""EPSG"",""9001""]],
                        AUTHORITY[""EPSG"",""3035""]]
                    "

                });

        public static Geometry ProjectTo(this Geometry geometry, int srid)
        {
            var transformation = _coordinateSystemServices.CreateTransformation(geometry.SRID, srid);

            var result = geometry.Copy();
            result.Apply(new MathTransformFilter((MathTransform)transformation.MathTransform));

            return result;
        }

        public class MathTransformFilter : ICoordinateSequenceFilter
        {
            readonly MathTransform _transform;

            public MathTransformFilter(MathTransform transform)
                => _transform = transform;

            public bool Done => false;
            public bool GeometryChanged => true;

            public void Filter(CoordinateSequence seq, int i)
            {
                var result = _transform.Transform(
                    new[]
                    {
                seq.GetOrdinate(i, Ordinate.X),
                seq.GetOrdinate(i, Ordinate.Y)
                    });
                seq.SetOrdinate(i, Ordinate.X, result[0]);
                seq.SetOrdinate(i, Ordinate.Y, result[1]);
            }
        }
    }
}
