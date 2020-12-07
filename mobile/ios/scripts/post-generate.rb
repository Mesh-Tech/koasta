run do |spec, proj|
  puts "Updating region information from " + proj.root_object.development_region + " to en-GB"

  proj.root_object.development_region = 'en-GB'
  proj.root_object.known_regions = ["en-GB", "Base"]

  proj.save()
end
