using System;

namespace com.swecial.unity {
	public struct DateRange {
		public DateTimeOffset fromDate, toDate;
		public DateRange(string sDates) {
			if (!sDates.isEmpty()) {
				if (sDates.Contains("-")) {
					string[] arrDates = sDates.Split('-');
					fromDate = arrDates[0].toDate();
					toDate = arrDates[1].toDate();
				} else {
					fromDate = sDates.toDate();
					toDate = sDates.toDate();
				}
				if (fromDate > DateTimeOffset.MinValue) {
					fromDate = fromDate.StartOfDay();
				}
				if (toDate > DateTimeOffset.MinValue) {
					toDate = toDate.EndOfDay();
				}
			} else {
				fromDate = DateTimeOffset.MinValue;
				toDate = DateTimeOffset.MinValue;
			}
		}
		public static DateRange parse(string sDates) {
			return new DateRange(sDates);
		}
		public bool isValid() {
			return isValid(DateTimeOffset.Now);
		}
		public bool isValid(DateTimeOffset date) {
			return  (fromDate <= date && toDate >= date);
		}
		public bool isEmpty() {
			return (fromDate == DateTimeOffset.MinValue && toDate == DateTimeOffset.MinValue);
		}
	}
}

