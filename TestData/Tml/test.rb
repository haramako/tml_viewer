#!/usr/bin/env ruby

require 'sinatra'

set :public_folder, File.dirname(__FILE__) + '/'

get '/:name.html' do
  name = params[:name]
  out = IO.popen("mono ../Tml/bin/Debug/Tml.exe #{name}.tml"){|f| f.read}
  out
end
