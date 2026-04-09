using AstrhoApp.API.Data;
using AstrhoApp.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AstrhoApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly AstrhoAppDbContext _context;

        public DashboardController(AstrhoAppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<DashboardOverviewDto>> GetDashboardData([FromQuery] string period = "today")
        {
            DateTime startDate = DateTime.Today;
            DateTime endDate = DateTime.Now;

            switch (period.ToLower())
            {
                case "week":
                    startDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                    break;
                case "month":
                    startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                    break;
                case "today":
                default:
                    startDate = DateTime.Today;
                    break;
            }

            var dashboardData = new DashboardOverviewDto();

            // 1. Real-Time Stats
            dashboardData.RealTimeStats = new RealTimeStatsDto
            {
                ActiveClients = await _context.Agenda.CountAsync(a => a.FechaCita == DateOnly.FromDateTime(DateTime.Today)),
                TodayAppointments = await _context.Agenda.CountAsync(a => a.FechaCita == DateOnly.FromDateTime(DateTime.Today)),
                PendingOrders = await _context.Agenda.CountAsync(a => a.Estado != null && (a.Estado.Nombre == "Pendiente" || a.Estado.Nombre == "Programada")),
                LowStockItems = await _context.Insumos.CountAsync(i => i.Stock < 10)
            };

            // 2. Summary Stats for the period
            var periodVentas = _context.Venta.Where(v => v.FechaRegistro >= startDate && v.FechaRegistro <= endDate);
            var periodAgenda = _context.Agenda.Where(a => a.FechaCita >= DateOnly.FromDateTime(startDate) && a.FechaCita <= DateOnly.FromDateTime(endDate));

            dashboardData.SummaryStats = new SummaryStatsDto
            {
                Revenue = await periodVentas.SumAsync(v => v.Total),
                Appointments = await periodAgenda.CountAsync(),
                Clients = await periodAgenda.Select(a => a.DocumentoCliente).Distinct().CountAsync(),
                ProductsSold = await _context.ServicioAgenda.Where(sa => periodAgenda.Select(a => a.AgendaId).Contains(sa.AgendaId)).CountAsync(),
                ServicesCompleted = await periodAgenda.CountAsync(a => a.Estado != null && (a.Estado.Nombre == "Completada" || a.Estado.Nombre == "Finalizada")),
                NewClients = 0
            };

            // 3. Revenue Chart
            if (period == "today")
            {
                // Group by hour for today - fetch data first, then format client-side
                var revenueData = await periodVentas
                    .Where(v => v.FechaRegistro != null)
                    .GroupBy(v => v.FechaRegistro!.Value.Hour)
                    .Select(g => new { Hour = g.Key, Total = g.Sum(v => v.Total) })
                    .OrderBy(c => c.Hour)
                    .ToListAsync();

                dashboardData.RevenueChart = revenueData
                    .Select(r => new ChartDataDto { Name = $"{r.Hour}:00", Value = r.Total })
                    .ToList();
            }
            else if (period == "week")
            {
                // Group by day of week
                var revenueData = periodVentas
                    .Where(v => v.FechaRegistro != null)
                    .AsEnumerable()
                    .GroupBy(v => v.FechaRegistro!.Value.ToString("ddd"))
                    .Select(g => new { Day = g.Key, Total = g.Sum(v => v.Total) })
                    .ToList();

                dashboardData.RevenueChart = revenueData
                    .Select(r => new ChartDataDto { Name = r.Day, Value = r.Total })
                    .ToList();
            }
            else
            {
                // Group by week of month
                var revenueData = periodVentas
                    .Where(v => v.FechaRegistro != null)
                    .AsEnumerable()
                    .GroupBy(v => (v.FechaRegistro!.Value.Day - 1) / 7 + 1)
                    .Select(g => new { Week = g.Key, Total = g.Sum(v => v.Total) })
                    .OrderBy(c => c.Week)
                    .ToList();

                dashboardData.RevenueChart = revenueData
                    .Select(r => new ChartDataDto { Name = $"Sem {r.Week}", Value = r.Total })
                    .ToList();
            }

            // 4. Appointments Chart
            if (period == "today")
            {
                var appointmentData = await periodAgenda
                    .GroupBy(a => a.HoraInicio.Hour)
                    .Select(g => new { Hour = g.Key, Count = g.Count() })
                    .OrderBy(c => c.Hour)
                    .ToListAsync();

                dashboardData.AppointmentsChart = appointmentData
                    .Select(a => new ChartDataDto { Name = $"{a.Hour}:00", Value = (decimal)a.Count })
                    .ToList();
            }
            else if (period == "week")
            {
                var appointmentData = periodAgenda
                    .AsEnumerable()
                    .GroupBy(a => a.FechaCita.ToDateTime(TimeOnly.MinValue).ToString("ddd"))
                    .Select(g => new { Day = g.Key, Count = g.Count() })
                    .ToList();

                dashboardData.AppointmentsChart = appointmentData
                    .Select(a => new ChartDataDto { Name = a.Day, Value = (decimal)a.Count })
                    .ToList();
            }
            else
            {
                var appointmentData = periodAgenda
                    .AsEnumerable()
                    .GroupBy(a => (a.FechaCita.Day - 1) / 7 + 1)
                    .Select(g => new { Week = g.Key, Count = g.Count() })
                    .OrderBy(c => c.Week)
                    .ToList();

                dashboardData.AppointmentsChart = appointmentData
                    .Select(a => new ChartDataDto { Name = $"Sem {a.Week}", Value = (decimal)a.Count })
                    .ToList();
            }

            // 5. Clients Chart (Simplified for now: total vs recurrent)
            int totalClients = await periodAgenda.Select(a => a.DocumentoCliente).Distinct().CountAsync();
            // Assuming "New" means first time in this period (very simplified logic)
            dashboardData.ClientsChart = new List<ClientTypeChartDto>
            {
                new ClientTypeChartDto { Name = "Nuevos", Value = totalClients / 3, Color = "#ec4899" },
                new ClientTypeChartDto { Name = "Recurrentes", Value = totalClients - (totalClients / 3), Color = "#a855f7" }
            };

            // 6. Products/Services Chart (Top categories)
            dashboardData.ProductsChart = await _context.ServicioAgenda
                .Where(sa => periodAgenda.Select(a => a.AgendaId).Contains(sa.AgendaId))
                .GroupBy(sa => sa.Servicio.Nombre)
                .Select(g => new ChartDataDto { Name = g.Key, Value = g.Count() })
                .Take(5)
                .ToListAsync();

            // 7. Upcoming Appointments
            dashboardData.UpcomingAppointments = await _context.Agenda
                .Where(a => a.FechaCita >= DateOnly.FromDateTime(DateTime.Today))
                .OrderBy(a => a.FechaCita).ThenBy(a => a.HoraInicio)
                .Take(5)
                .Select(a => new UpcomingAppointmentDto
                {
                    Id = a.AgendaId,
                    Client = a.DocumentoClienteNavigation != null ? a.DocumentoClienteNavigation.Nombre : "Cliente desconocido",
                    Service = a.ServicioAgenda.FirstOrDefault() != null && a.ServicioAgenda.FirstOrDefault()!.Servicio != null 
                        ? a.ServicioAgenda.FirstOrDefault()!.Servicio.Nombre 
                        : "Sin servicio",
                    Time = $"{a.FechaCita:dd/MM} {a.HoraInicio:HH:mm}",
                    Status = a.Estado != null ? a.Estado.Nombre : "Pendiente",
                    Employee = a.DocumentoEmpleadoNavigation != null ? a.DocumentoEmpleadoNavigation.Nombre : "Sin asignar"
                })
                .ToListAsync();

            // 8. Top Services
            var topServicesData = await _context.ServicioAgenda
                .Where(sa => periodAgenda.Select(a => a.AgendaId).Contains(sa.AgendaId))
                .GroupBy(sa => sa.Servicio.Nombre)
                .Select(g => new { Name = g.Key, Count = g.Count(), Revenue = g.Sum(sa => sa.Servicio.Precio) })
                .OrderByDescending(s => s.Count)
                .Take(4)
                .ToListAsync();

            decimal totalTopRevenue = topServicesData.Sum(s => s.Revenue);

            dashboardData.TopServices = topServicesData.Select(s => new TopServiceDto
            {
                Name = s.Name,
                Count = s.Count,
                Revenue = s.Revenue,
                Percentage = totalTopRevenue > 0 ? (int)(s.Revenue * 100 / totalTopRevenue) : 0
            }).ToList();

            return Ok(new { success = true, data = dashboardData });
        }
    }
}
