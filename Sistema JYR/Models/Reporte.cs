using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sistema_JYR.Models
{
    public class Reporte
    {
        public static ReportViewer reporte<T>(List<T> data, string nombreDS, string nombreReporte)
        {
            nombreDS = (nombreDS == "") ? "DataSet1" : nombreDS;

            ReportViewer reportViewer = new ReportViewer();

            //reportViewer.LocalReport.ReportPath = HttpContext.Current.Server.MapPath(HttpContext.Current.Request.ApplicationPath);

            reportViewer.LocalReport.DataSources.Clear();
            ReportDataSource rdc = new ReportDataSource(nombreDS, data);

            reportViewer.LocalReport.DataSources.Add(rdc);

            reportViewer.LocalReport.Refresh();

            reportViewer.ProcessingMode = ProcessingMode.Local;
            reportViewer.LocalReport.ReportPath += @"Reportes/" + nombreReporte;
            reportViewer.Width = 900;
            reportViewer.Height = 600;
            reportViewer.ZoomMode = ZoomMode.PageWidth;
            return reportViewer;
        }
    }
}