## TODO:

### House management

- [ ] Task Creation and Management
- [ ] Database storage
- [ ] Deployment to cloud
- [ ] User Authentication
- [ ] More than one house, add houses
- [ ] Share to family members
- [ ] Task solution instructions
- [ ] All task text translations
- [ ] Links to external resources for task solutions
- [ ] Embedded videos for task instructions
- [ ] Links to products/tools needed for task completion (with affiliate marketing support)
- [ ] User-contributed solutions and tips
- [ ] Community forum for discussion and support
- [ ] SEO optimization
- [ ] Icons for items
- [ ] Yanitor Brand icon




return new List<HouseItem>
        {
            new HouseItem
            {
                Name = "Ventilation System",
                Type = "Ventilation",
                Room = "Other",
                RoomType = "Other",
                Tasks = taskProvider.GetTasksForItemType("Ventilation").ToList()
            },
            new HouseItem
            {
                Name = "Master Bathroom Shower",
                Type = "Shower",
                Room = "Master Bathroom",
                RoomType = "Bathroom",
                Tasks = taskProvider.GetTasksForItemType("Shower").ToList()
            },
            new HouseItem
            {
                Name = "Washing Machine",
                Type = "WashingMachine",
                Room = "Master Bathroom",
                RoomType = "Bathroom",
                Tasks = taskProvider.GetTasksForItemType("WashingMachine").ToList()
            },
            new HouseItem
            {
                Name = "Dishwasher",
                Type = "Dishwasher",
                Room = "Kitchen",
                RoomType = "Kitchen",
                Tasks = taskProvider.GetTasksForItemType("Dishwasher").ToList()
            }
        };