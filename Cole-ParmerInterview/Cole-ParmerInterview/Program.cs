using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interview.Problem1.Domain;
using Newtonsoft.Json;
using System.Net;

namespace Cole_ParmerInterview
{
    class Program
    {
        static void Main(string[] args)
        {

            StreamReader jsonFile = new StreamReader(@"c:\data\tripdata.json");
            TripVm TripData = JsonConvert.DeserializeObject<TripVm>(jsonFile.ReadToEnd());

            TimeSpan totalChannelAlarmDuration, totalChannelEventDuration, totalOverallDuration = new TimeSpan();

            foreach(var setting in TripData.TripSettings)
            {
                totalOverallDuration += totalChannelAlarmDuration = checkAlarms(setting.ChannelName, TripData);
                totalOverallDuration += totalChannelEventDuration = checkEvents(setting.ChannelName, TripData);
            }

            Console.WriteLine("\nTOTAL TIME IN ALARM : {0}", totalOverallDuration);

                Console.ReadKey(true);
        }

        static TimeSpan checkAlarms(string channel, TripVm TripData)
        {
            var UploadData = (from uploadData in TripData.TripUploadData where uploadData.Channel == channel select uploadData).OrderBy(ud => ud.Timestamp).ToList();

            DateTime? beginAlarm = null;
            TimeSpan alarmDuration, totalDuration = new TimeSpan();
            var MinSetting = TripData.TripSettings.Where(ts => ts.ChannelName == channel).FirstOrDefault().Min;
            var MaxSetting = TripData.TripSettings.Where(ts => ts.ChannelName == channel).FirstOrDefault().Max;

            foreach (var uploadData in UploadData)
            {
                if ((uploadData.Data < MinSetting || uploadData.Data > MaxSetting) && beginAlarm == null)
                {
                    Console.WriteLine("{0} - {1} : ", channel.ToUpper(), TripData.TripSettings.Where(ts => ts.ChannelName == channel).FirstOrDefault().DataType);
                    beginAlarm = uploadData.Timestamp;
                    Console.WriteLine("Entered Alarm @ {0}", beginAlarm.ToString());
                }
                if (uploadData.Data >= MinSetting && uploadData.Data <= MaxSetting && beginAlarm != null)
                {
                    Console.WriteLine("Exited Alarm @ {0}", uploadData.Timestamp.ToString());
                    alarmDuration = uploadData.Timestamp.Subtract((DateTime)beginAlarm);
                    Console.WriteLine("\t\t\t{0} Alarm Duration : {1}\n", channel, alarmDuration.ToString());
                    beginAlarm = null;

                    totalDuration += alarmDuration;
                }
            }

            // Checks if there are still Alarms at Collection Ending
            if(beginAlarm != null)
            {
                alarmDuration = TripData.EndTime.Subtract((DateTime)beginAlarm);
                totalDuration += alarmDuration;
                Console.WriteLine("Exited Alarm @ {0}", TripData.EndTime.ToString());
                Console.WriteLine("\t\t\t{0} Alarm Duration : {1}\n", channel, alarmDuration.ToString());
                beginAlarm = null;
            }

            Console.WriteLine("\t\t\t\tTotal {0} Alarm duration : {1}\n", channel, totalDuration.ToString());
            return totalDuration;
        }

        static TimeSpan checkEvents(string channel, TripVm TripData)
        {
            var uploadEvents = (from uploadEvent in TripData.TripUploadEvents where uploadEvent.Channel == channel select uploadEvent).OrderBy(ud => ud.Timestamp).ToList();

            DateTime? beginAlarm = null;
            TimeSpan alarmDuration, totalDuration = new TimeSpan();

            foreach (var uploadEvent in uploadEvents)
            {
                if (((AlarmState)uploadEvent.EventType == AlarmState.MaxAlarmOut || (AlarmState)uploadEvent.EventType == AlarmState.MinAlarmOut) && beginAlarm == null)
                {
                    Console.WriteLine("{0} - {1} : ", channel.ToUpper(), TripData.TripSettings.Where(ts => ts.ChannelName == channel).FirstOrDefault().DataType);
                    beginAlarm = uploadEvent.Timestamp;
                    Console.WriteLine("Entered Event @ {0}", beginAlarm.ToString());
                }
                if (((AlarmState)uploadEvent.EventType == AlarmState.MaxAlarmIn || (AlarmState)uploadEvent.EventType == AlarmState.MinAlarmIn) && beginAlarm != null)
                {
                    Console.WriteLine("Exited Event @ {0}", uploadEvent.Timestamp.ToString());
                    alarmDuration = uploadEvent.Timestamp.Subtract((DateTime)beginAlarm);
                    Console.WriteLine("\t\t\t{0} Event Duration : {1}\n", channel, alarmDuration.ToString());
                    beginAlarm = null;

                    totalDuration += alarmDuration;
                }
            }

            // Checks if there are still Events Open at Collection Ending
            if (beginAlarm != null)
            {
                alarmDuration = TripData.EndTime.Subtract((DateTime)beginAlarm);
                totalDuration += alarmDuration;
                Console.WriteLine("Exited Event @ {0}", TripData.EndTime.ToString());
                Console.WriteLine("\t\t\t{0} Event Duration : {1}\n", channel, alarmDuration.ToString());
                beginAlarm = null;
            }

            Console.WriteLine("\t\t\t\tTotal {0} Event duration : {1}\n", channel, totalDuration.ToString());
            return totalDuration;
        }

    }
}
