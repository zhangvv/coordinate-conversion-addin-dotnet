﻿using ArcGIS.Core.Geometry;
using CoordinateToolLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProAppCoordToolModule
{
    public class ProCoordinateGet : CoordinateToolLibrary.Models.CoordinateGetBase
    {
        public ProCoordinateGet()
        { }

        public MapPoint Point { get; set; }

        #region Can Gets

        //public override bool CanGetDD(out string coord)
        //{
        //    coord = string.Empty;
        //    if (Point != null)
        //    {
        //        try
        //        {
        //            var cn = Point as IConversionNotation;
        //            coord = cn.GetDDFromCoords(6);
        //            return true;
        //        }
        //        catch { }
        //    }
        //    return false;
        //}

        public override bool CanGetDDM(out string coord)
        {
            coord = string.Empty;
            if(base.CanGetDDM(out coord))
            {
                return true;
            }
            else
            {
                if(base.CanGetDD(out coord))
                {
                    // convert dd to ddm
                    CoordinateDD dd;
                    if(CoordinateDD.TryParse(coord, out dd))
                    {
                        var ddm = new CoordinateDDM(dd);
                        coord = ddm.ToString("", new CoordinateDDMFormatter());
                        return true;
                    }
                }
            }
            return false;
        }

        public override bool CanGetDMS(out string coord)
        {
            coord = string.Empty;
            if (base.CanGetDMS(out coord))
            {
                return true;
            }
            else
            {
                if (base.CanGetDD(out coord))
                {
                    // convert dd to ddm
                    CoordinateDD dd;
                    if (CoordinateDD.TryParse(coord, out dd))
                    {
                        var dms = new CoordinateDMS(dd);
                        coord = dms.ToString("", new CoordinateDMSFormatter());
                        return true;
                    }
                }
            }
            return false;
        }

        //public override bool CanGetGARS(out string coord)
        //{
        //    coord = string.Empty;
        //    if (Point != null)
        //    {
        //        try
        //        {
        //            var cn = Point as IConversionNotation;
        //            coord = cn.GetGARSFromCoords();
        //            return true;
        //        }
        //        catch { }
        //    }
        //    return false;
        //}

        //public override bool CanGetMGRS(out string coord)
        //{
        //    coord = string.Empty;
        //    if (Point != null)
        //    {
        //        try
        //        {
        //            // 5 numeric units in MGRS is 1m resolution
        //            var cn = Point as IConversionNotation;
        //            coord = cn.CreateMGRS(5, false, esriMGRSModeEnum.esriMGRSMode_NewStyle);
        //            return true;
        //        }
        //        catch { }
        //    }
        //    return false;
        //}

        //public override bool CanGetUSNG(out string coord)
        //{
        //    coord = string.Empty;
        //    if (Point != null)
        //    {
        //        try
        //        {
        //            var cn = Point as IConversionNotation;
        //            coord = cn.GetUSNGFromCoords(5, false, false);
        //            return true;
        //        }
        //        catch { }
        //    }
        //    return false;
        //}

        //public override bool CanGetUTM(out string coord)
        //{
        //    coord = string.Empty;
        //    if (Point != null)
        //    {
        //        try
        //        {
        //            var cn = Point as IConversionNotation;
        //            coord = cn.GetUTMFromCoords(esriUTMConversionOptionsEnum.esriUTMAddSpaces|esriUTMConversionOptionsEnum.esriUTMUseNS);
        //            return true;
        //        }
        //        catch { }
        //    }
        //    return false;
        //}

        #endregion
    }
}
