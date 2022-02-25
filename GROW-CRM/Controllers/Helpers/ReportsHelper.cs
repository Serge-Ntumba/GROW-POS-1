﻿using GROW_CRM.Data;
using GROW_CRM.Models;
using GROW_CRM.ViewModels.ReportsViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GROW_CRM.Controllers.Helpers
{
    public static class ReportsHelper
    {
        public static List<IEnumerable> GetDemoData(GROWContext _context)
        {
            var members = from m in _context.Members
                              .Include(m => m.Gender)
                          select m;

            int memberCount = members.Count();

            var genderReport = _context.Members
                                    .Include(m => m.Gender)
                                    .GroupBy(m => new { m.Gender.Name })
                                    .Select(grp => new GenderReport
                                    {
                                        Gender = grp.Key.Name,
                                        Percentage = 0,
                                        Total = grp.Count()
                                    }).ToList();

            for (int i = 0; i < genderReport.Count(); i++)
            {
                genderReport[i].Percentage = Math.Round((double)genderReport[i].Total/memberCount, 2);
                genderReport[i].PercentageText = $"{Math.Round(genderReport[i].Percentage * 100, 2)}%";
            }

            DateTime now = DateTime.Now;            

            int[] totals = new int[] { 0, 0, 0, 0};

            foreach (Member m in members)
            {
                TimeSpan diff = (TimeSpan)(now - m.DOB);
                int diffYears = (int)Math.Round(diff.TotalDays / 365);

                if (diffYears < 13) totals[0] += 1;
                else if (diffYears < 19) totals[1] += 1;
                else if (diffYears < 65) totals[2] += 1;
                else totals[3] += 1;
            }

            List<AgeReport> ageReport = new List<AgeReport> {
                new AgeReport{ AgeRange = "0-12", Percentage = 0, PercentageText = "", Total = 0},
                new AgeReport{ AgeRange = "13-18", Percentage = 0, PercentageText = "", Total = 0},
                new AgeReport{ AgeRange = "19-64", Percentage = 0, PercentageText = "", Total = 0},
                new AgeReport{ AgeRange = "65+", Percentage = 0, PercentageText = "", Total = 0},
            };

            for (int i = 0; i < ageReport.Count(); i++)
            {
                ageReport[i].Total = totals[i];
                double per = Math.Round((double)(totals[i] / memberCount), 2);
                ageReport[i].Percentage = per;
                ageReport[i].PercentageText = $"{per * 100}%";
            }

            var dietaryTotal = _context.DietaryRestrictionMembers.Count();

            var dietaryReport = _context.DietaryRestrictionMembers
                                .Include(dr => dr.DietaryRestriction)
                                .GroupBy(dr => new { dr.DietaryRestriction.Restriction })
                                .Select(grp => new DietaryReport
                                {
                                    Restriction = grp.Key.Restriction,
                                    Percentage = grp.Count(),
                                    Total = grp.Count()
                                }).ToList();

            for (int i = 0; i < dietaryReport.Count(); i++)
            {
                dietaryReport[i].Percentage = Math.Round(dietaryReport[i].Percentage/dietaryTotal, 2);
                dietaryReport[i].PercentageText = $"{dietaryReport[i].Percentage * 100}%";
            }

            return new List<IEnumerable> { genderReport, ageReport, dietaryReport };
        }

        public static IEnumerable GetCitiesData(GROWContext _context)
        {
            var householdCount = _context.Households.Count();

            var citiesReport = _context.Households
                        .Include(h => h.City)
                        .GroupBy(h => new { h.City.Name })
                        .Select(grp => new CitiesReport
                        {
                            Name = grp.Key.Name,
                            Percentage = grp.Count(),
                            Total = grp.Count()
                        }).ToList();

            for (int i = 0; i < citiesReport.Count(); i++)
            {
                citiesReport[i].Percentage /= householdCount;
            }

            return citiesReport;
        }

        public static IEnumerable GetCityReports(GROWContext _context)
        {
            List<List<CityReport>> cityReports = new List<List<CityReport>>();

            var cities = _context.Cities.ToList();

            //This can get better
            //Tripple Loops are not a good idea
            foreach (City c in cities)
            {
                var h = _context.Members
                        .Include(h => h.MemberIncomeSituations)
                        .Include(h => h.Household).ThenInclude(hh => hh.City)
                        .Where(h => h.Household.City.Name == c.Name)
                        .GroupBy(h => new { h.Household.PostalCode, h.Household.City.Name })
                        .Select(grp => new CityReport
                        {
                            Name = grp.Key.Name,
                            PostalCode = grp.Key.PostalCode,
                            NumberOfMembers = grp.Count(),
                            TotalIncome = 0//grp.Sum(h => )
                        }).ToList();

                for (int i = 0; i < h.Count(); i++)
                {
                    CityReport cr = h.ElementAt(i);

                    var members = _context.Members
                                  .Include(m => m.MemberIncomeSituations)
                                  .Include(m => m.Household).ThenInclude(h => h.City)
                                  .Where(m => m.Household.City.Name == cr.Name && m.Household.PostalCode == cr.PostalCode)
                                  .Select(m => m).ToList();

                    double inc = 0;

                    foreach (Member m in members)
                    {
                        inc += m.YearlyIncome;
                    }

                    cr.TotalIncome = inc;
                }

                cityReports.Add(h);
            }

            return cityReports;
        }
    }
}
