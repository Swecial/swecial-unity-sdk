using System;
using System.Collections.Generic;

namespace com.swecial.unity {
	public class DateRanges {
		public List<DateRange> dateRanges;
		public DateRanges(string sRanges) {
			dateRanges = new List<DateRange>();
			if (!sRanges.isEmpty()) {
				sRanges = sRanges.Replace(" ", "");
				string[] arrDateGroups = sRanges.Split(',');
				foreach (string dateGroup in arrDateGroups) {
					var dateRange = DateRange.parse(dateGroup);
					dateRanges.Add(dateRange);
				}
			}
		}
		public static DateRanges parse(string sRanges) {
			return new DateRanges(sRanges);
		}
		public bool isValid() {
			return isValid(DateTimeOffset.Now);
		}
		public bool isValid(DateTimeOffset date) {
			foreach (var dateRange in dateRanges) {
				if (dateRange.isValid(date)) {
					return true;
				}
			}
			return false;
		}
		public bool isEmpty() {
			if (dateRanges != null && dateRanges.Count > 0) {
				foreach (var dateRange in dateRanges) {
					if (!dateRange.isEmpty()) {
						return false;
					}
				}
				return true;
			} else {
				return true;
			}
		}
	}
}

