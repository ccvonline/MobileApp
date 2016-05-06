using System;
using System.Collections.Generic;
using Rock.Mobile.Util.Strings;
using App.Shared.Network;
using System.Collections;
using App.Shared.Strings;
using App.Shared.PrivateConfig;
using Rock.Mobile.Network;
using MobileApp;
using System.Linq;

namespace App.Shared
{
    public class GroupFinder
    {
        public GroupFinder( )
        {
        }

        public class GroupEntry
        {
            public string Title { get; set; }
            public string Address { get; set; }

            //public string Day { get; set; }
            //public string Time { get; set; }

            public string MeetingTime { get; set; }

            public string NeighborhoodArea { get; set; }

            public double Distance { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }

            public int Id { get; set; }
        }

        public delegate void GetGroupsComplete( GroupEntry sourceLocation, List<GroupEntry> groupEntry, bool result );
        public static void GetGroups( string street, string city, string state, string zip, int skip, int top, GetGroupsComplete onCompletion )
        {
            List<GroupEntry> groupEntries = new List<GroupEntry>();
            GroupEntry sourceLocation = new GroupEntry( );

            // first convert the address into a location object
            RockApi.Get_Locations_FromAddress( street, city, state, zip, 
                delegate(System.Net.HttpStatusCode statusCode, string statusDescription, Rock.Client.Location model )
                {
                    // verify the call was successful AND it resulted in a valid location
                    if ( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true && 
                        model != null &&
                        model.Latitude.HasValue == true && 
                        model.Longitude.HasValue == true )
                    { 
                        // take the source location so we can provide it to the caller
                        sourceLocation.Latitude = model.Latitude.Value;
                        sourceLocation.Longitude = model.Longitude.Value;

                        // now get the groups
                        MobileAppApi.GetPublicGroupsByLocation( PrivateGeneralConfig.GroupType_NeighborhoodGroupId, model.Id, skip, top,
                            delegate(System.Net.HttpStatusCode groupStatusCode, string groupStatusDescription, List<Rock.Client.Group> rockGroupList )
                            {
                                bool result = false;

                                if ( Rock.Mobile.Network.Util.StatusInSuccessRange( groupStatusCode ) == true )
                                {
                                    result = true;

                                    // go through each small group
                                    foreach ( Rock.Client.Group smallGroup in rockGroupList )
                                    {
                                        // only take public groups
                                        if ( smallGroup.IsPublic == true )
                                        {
                                            // get the group location out of the small group enumerator
                                            IEnumerator enumerator = smallGroup.GroupLocations.GetEnumerator( );
                                            enumerator.MoveNext( );
                                            Rock.Client.Location location = ( (Rock.Client.GroupLocation)enumerator.Current ).Location;

                                            // and of course, each group has a location
                                            GroupEntry entry = new GroupEntry();
                                            entry.Title = smallGroup.Name;
                                            entry.Address = location.Street1 + "\n" + location.City + ", " + location.State + " " + location.PostalCode.Substring( 0, Math.Max( 0, location.PostalCode.IndexOf( '-' ) ) );
                                            entry.NeighborhoodArea = smallGroup.Name;
                                            entry.Id = smallGroup.Id;

                                            // get the distance 
                                            entry.Distance = location.Distance;
                                            entry.Latitude = location.Latitude.Value;
                                            entry.Longitude = location.Longitude.Value;

                                            // get the meeting schedule if it's available
                                            if ( smallGroup.Schedule != null )
                                            {
                                                entry.MeetingTime = smallGroup.Schedule.FriendlyScheduleText;
                                            }

                                            // make sure the group hasn't already been added.
                                            GroupEntry duplicateGroup = groupEntries.Where( g => g.Id == entry.Id ).SingleOrDefault( );

                                            if( duplicateGroup == null )
                                            {
                                                groupEntries.Add( entry );
                                            }
                                        }
                                    }
                                }

                                // our network delegate has been invoked and compelted, so now call whoever called us.
                                onCompletion( sourceLocation, groupEntries, result );
                            } );
                    }
                    else
                    {
                        onCompletion( sourceLocation, groupEntries, Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true ? true : false );
                    }
                } );
        }

        static DateTime ParseMilitaryTime(string time )
        {
            //
            // Convert hour part of string to integer.
            //
            string hour = time.Substring(0, 2);
            int hourInt = int.Parse(hour);
            if (hourInt >= 24)
            {
                throw new ArgumentOutOfRangeException("Invalid hour");
            }
            //
            // Convert minute part of string to integer.
            //
            string minute = time.Substring(3, 2);
            int minuteInt = int.Parse(minute);
            if (minuteInt >= 60)
            {
                throw new ArgumentOutOfRangeException("Invalid minute");
            }
            //
            // Return the DateTime.
            //
            return new DateTime(2000, 1, 1, hourInt, minuteInt, 0);
        }
    }
}

